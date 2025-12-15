#!/bin/bash

# Script để setup ML Generation cho RhythmGame3D
# Chạy script này để cài đặt tất cả dependencies

echo "======================================"
echo "  RhythmGame3D - ML Setup Script"
echo "======================================"
echo ""

# Kiểm tra Python
echo "1️⃣  Checking Python installation..."
if ! command -v python3 &> /dev/null; then
    echo "❌ Python3 not found. Please install Python 3.8+"
    exit 1
fi

PYTHON_VERSION=$(python3 --version)
echo "✅ Found: $PYTHON_VERSION"
echo ""

# Tạo virtual environment
echo "2️⃣  Creating virtual environment..."
cd "$(dirname "$0")"

if [ -d "ml_env" ]; then
    echo "⚠️  Virtual environment already exists. Removing..."
    rm -rf ml_env
fi

python3 -m venv ml_env
echo "✅ Virtual environment created"
echo ""

# Activate virtual environment
echo "3️⃣  Activating virtual environment..."
source ml_env/bin/activate
echo "✅ Activated: $(which python)"
echo ""

# Upgrade pip
echo "4️⃣  Upgrading pip..."
pip install --upgrade pip --quiet
echo "✅ Pip upgraded"
echo ""

# Install dependencies
echo "5️⃣  Installing ML dependencies..."
echo "   This may take 5-10 minutes (first time)..."
echo ""

pip install torch --index-url https://download.pytorch.org/whl/cpu
pip install librosa
pip install huggingface-hub
pip install numpy

echo ""
echo "✅ All dependencies installed!"
echo ""

# Try to install beatlearning
echo "6️⃣  Installing beatlearning..."
pip install beatlearning

if [ $? -ne 0 ]; then
    echo "⚠️  beatlearning package not available via pip"
    echo "   You may need to install from source or use alternative"
fi

echo ""
echo "======================================"
echo "  ✅ ML Setup Complete!"
echo "======================================"
echo ""
echo "📝 Next steps:"
echo "   1. Test generation:"
echo "      source ml_env/bin/activate"
echo "      python generate_beatmap.py <audio.mp3> 0.5 output.osu"
echo ""
echo "   2. Run Unity game with useMLGeneration = true"
echo ""
echo "======================================"
