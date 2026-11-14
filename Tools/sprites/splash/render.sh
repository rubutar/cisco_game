#!/bin/sh
# Renders the splash art with headless Chrome (fonts come from Google Fonts, so it needs the network):
#   Assets/_Project/Art/Splash/byte_world_splash.png      1920 x 1080 with the BYTE WORLD wordmark and tagline
#                                                        (itch.io cover, video, WebGL loading screen)
#   Assets/_Project/Resources/Sprites/UI/title_backdrop.png   the same picture without text, for the title
#                                                        screen (its title and tagline stay in dialogue.json)
# The town is the hi-fi map on plain grass (make_town.py, run first); Pax comes from Tools/sprites/hifi/. Run from the repo root: sh Tools/sprites/splash/render.sh
set -e
CHROME="/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"
DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$DIR/../../.."
render() {
  "$CHROME" --headless=new --disable-gpu --hide-scrollbars --allow-file-access-from-files \
    --window-size=1920,1080 --virtual-time-budget=5000 --screenshot="$2" "file://$DIR/$1.html" 2>/dev/null
}
python3 "$DIR/make_town.py"
render splash "$ROOT/Assets/_Project/Art/Splash/byte_world_splash.png"
render backdrop "$ROOT/Assets/_Project/Resources/Sprites/UI/title_backdrop.png"
echo "splash art written"
