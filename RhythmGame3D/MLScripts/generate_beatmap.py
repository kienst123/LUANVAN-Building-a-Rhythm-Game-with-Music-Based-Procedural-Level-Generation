#!/usr/bin/env python3
"""
Generate Beatmap Script for Unity Integration
Uses BEaRT ML model to generate beatmaps from MP3 files

Usage:
    python generate_beatmap.py <audio_path> <difficulty> <output_path>
    
Arguments:
    audio_path: Path to MP3/OGG/WAV file
    difficulty: Difficulty value (0.0 = easy, 1.0 = hard)
    output_path: Path to save output .osu file
    
Example:
    python generate_beatmap.py song.mp3 0.5 output.osu
"""

import sys
import os
import json
from pathlib import Path

# Add BeatLearning source code to Python path
beatlearning_path = "/Volumes/KIEN 4TB/LuanVan copy/another/BeatLearning"
if beatlearning_path not in sys.path:
    sys.path.insert(0, beatlearning_path)

# Verify path was added
print(f"DEBUG: Added to sys.path: {beatlearning_path}", file=sys.stderr, flush=True)
print(f"DEBUG: Path exists: {os.path.exists(beatlearning_path)}", file=sys.stderr, flush=True)

def print_progress(stage, message):
    """Print progress with JSON format for Unity to parse"""
    progress_data = {
        "stage": stage,
        "message": message,
        "timestamp": __import__('time').time()
    }
    print(f"PROGRESS:{json.dumps(progress_data)}", flush=True)

def generate_beatmap(audio_path, difficulty, output_path):
    """
    Generate beatmap using BEaRT ML model
    
    Args:
        audio_path (str): Path to audio file
        difficulty (float): Difficulty from 0.0 to 1.0
        output_path (str): Output .osu file path
        
    Returns:
        int: Exit code (0 = success, 1 = error)
    """
    try:
        # Stage 1: Import dependencies
        print_progress("import", "Loading Python libraries...")
        
        try:
            from huggingface_hub import hf_hub_download
            from beatlearning.tokenizers import BEaRTTokenizer
            from beatlearning.configs import QuaverBEaRT
            from beatlearning.models import BEaRT
            from beatlearning.converters import OsuBeatmapConverter
        except ImportError as e:
            print(f"ERROR: Missing required library: {e}", file=sys.stderr)
            print("Please install: pip install beatlearning torch librosa huggingface_hub", file=sys.stderr)
            return 1
        
        # Stage 2: Validate inputs
        print_progress("validate", "Validating input files...")
        
        if not os.path.exists(audio_path):
            print(f"ERROR: Audio file not found: {audio_path}", file=sys.stderr)
            return 1
        
        if not (0.0 <= difficulty <= 1.0):
            print(f"ERROR: Difficulty must be between 0.0 and 1.0, got {difficulty}", file=sys.stderr)
            return 1
        
        # Create output directory if needed
        output_dir = os.path.dirname(output_path)
        if output_dir and not os.path.exists(output_dir):
            os.makedirs(output_dir, exist_ok=True)
        
        # Stage 3: Load model
        print_progress("download", "Downloading model from HuggingFace...")
        
        # Cache directory for model (Unity StreamingAssets or temp)
        cache_dir = os.path.join(os.path.dirname(__file__), "ml_cache")
        os.makedirs(cache_dir, exist_ok=True)
        
        checkpoint = hf_hub_download(
            repo_id="sedthh/BeatLearning",
            filename="quaver_beart_v1.pt",
            cache_dir=cache_dir
        )
        
        print_progress("load_model", "Loading BEaRT model...")
        
        # Determine device first
        import torch
        device = "cuda" if torch.cuda.is_available() else "cpu"
        print_progress("device", f"Using device: {device}")
        
        # Initialize tokenizer and model
        tokenizer = BEaRTTokenizer(QuaverBEaRT())
        model = BEaRT(tokenizer)
        
        # Load model with map_location for CPU compatibility and weights_only=False for legacy models
        model.load(checkpoint, map_location=torch.device(device), weights_only=False)
        model.to(device)
        
        # Stage 4: Generate beatmap
        print_progress("generate", f"Generating beatmap (difficulty={difficulty:.2f})...")
        
        # OPTIMIZED PARAMETERS for faster CPU generation:
        # - Reduced beams: [2]*8 → [2]*4 (balanced)
        # - Reduced max_beam_width: 256 → 64 (4x faster)
        # - Higher temperature: 0.1 → 0.2 (slightly more randomness)
        # Trade-off: Slightly lower quality but much faster for CPU
        
        # FULL SONG GENERATION
        # Note: This will take 3-6 minutes for a 3-4 minute song on CPU
        # If you want faster testing, change audio_end_limit to 90.0
        audio_end_limit = None  # Generate full song (changed from 90.0)
        
        ibf = model.generate(
            audio_file=audio_path,
            audio_start=0.0,
            audio_end=audio_end_limit,  # None = full song
            use_tracks=["LEFT"],  # OSU mode uses only LEFT track
            difficulty=float(difficulty),
            beams=[2] * 4,  # Reduced from [2]*8 for speed
            max_beam_width=64,  # Reduced from 256 for speed
            temperature=0.2,  # Increased from 0.1 for speed
            random_seed=42  # Fixed seed for reproducibility
        )
        
        print_progress("save_ibf", "Saving intermediate beatmap format...")
        
        # Stage 5: Save IBF to temporary file first
        temp_ibf_path = output_path.replace(".osu", ".ibf")
        ibf.save(temp_ibf_path)
        
        print_progress("convert", "Converting to .osu format...")
        
        # Stage 6: Convert IBF to .osu format
        converter = OsuBeatmapConverter()
        
        # Extract metadata from filename
        audio_filename = os.path.basename(audio_path)
        song_name = os.path.splitext(audio_filename)[0]
        
        # Map difficulty to names
        if difficulty < 0.35:
            diff_name = "Easy"
            overall_diff = 3
        elif difficulty < 0.6:
            diff_name = "Normal"
            overall_diff = 5
        else:
            diff_name = "Hard"
            overall_diff = 7
        
        # Generate .osu file from IBF file
        converter.generate(temp_ibf_path, output_path, meta={
            "title": song_name,
            "artist": "Unknown Artist",
            "creator": "BeatLearning AI",
            "difficulty_name": diff_name,
            "overall_difficulty": overall_diff,
            "hp_drain_rate": overall_diff,
            "circle_size": 4,  # 4K mode
            "approach_rate": overall_diff,
        })
        
        # Clean up temporary IBF file
        if os.path.exists(temp_ibf_path):
            os.remove(temp_ibf_path)
        
        # Stage 6: Verify output
        print_progress("verify", "Verifying generated file...")
        
        if not os.path.exists(output_path):
            print(f"ERROR: Failed to generate output file", file=sys.stderr)
            return 1
        
        # Get file size for verification
        file_size = os.path.getsize(output_path)
        if file_size < 100:  # .osu file should be at least 100 bytes
            print(f"ERROR: Generated file is too small ({file_size} bytes)", file=sys.stderr)
            return 1
        
        # Success!
        print_progress("complete", f"Beatmap generated successfully! ({file_size} bytes)")
        print(f"SUCCESS:{output_path}", flush=True)
        
        return 0
        
    except Exception as e:
        import traceback
        error_msg = f"Unexpected error: {str(e)}"
        print(f"ERROR: {error_msg}", file=sys.stderr)
        print(traceback.format_exc(), file=sys.stderr)
        return 1

def main():
    """Main entry point"""
    # Check arguments
    if len(sys.argv) != 4:
        print("Usage: python generate_beatmap.py <audio_path> <difficulty> <output_path>", file=sys.stderr)
        print("", file=sys.stderr)
        print("Arguments:", file=sys.stderr)
        print("  audio_path  - Path to MP3/OGG/WAV file", file=sys.stderr)
        print("  difficulty  - Difficulty (0.0-1.0)", file=sys.stderr)
        print("  output_path - Output .osu file path", file=sys.stderr)
        print("", file=sys.stderr)
        print("Example:", file=sys.stderr)
        print("  python generate_beatmap.py song.mp3 0.5 output.osu", file=sys.stderr)
        sys.exit(1)
    
    # Parse arguments
    audio_path = sys.argv[1]
    try:
        difficulty = float(sys.argv[2])
    except ValueError:
        print(f"ERROR: Invalid difficulty value: {sys.argv[2]}", file=sys.stderr)
        sys.exit(1)
    
    output_path = sys.argv[3]
    
    # Print info
    print(f"BeatLearning ML Beatmap Generator", flush=True)
    print(f"Audio: {audio_path}", flush=True)
    print(f"Difficulty: {difficulty}", flush=True)
    print(f"Output: {output_path}", flush=True)
    print(f"---", flush=True)
    
    # Generate
    exit_code = generate_beatmap(audio_path, difficulty, output_path)
    
    sys.exit(exit_code)

if __name__ == "__main__":
    main()
