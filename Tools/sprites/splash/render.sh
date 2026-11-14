#!/bin/sh
# Renders the splash art with headless Chrome (fonts come from Google Fonts, so it needs the network):
#   Assets/_Project/Art/Splash/byte_world_splash.png      1920 x 1080 with the BYTE WORLD wordmark and tagline
#                                                        (itch.io cover, video, WebGL loading screen)
#   Assets/_Project/Resources/Sprites/UI/title_backdrop.png   the same picture without text, for the title
#                                                        screen (its title and tagline stay in dialogue.json)
#   Assets/_Project/Resources/Sprites/UI/logo.png         1600 x 260 BYTE WORLD wordmark on transparent, for the
#                                                        title screen (the brand is art; the tagline stays text)
#   Assets/_Project/Resources/Sprites/UI/intro_<n>.png    960 x 540 story scenes, one per dialogue.json "intro"
#                                                        line in order (make_intro.py writes their HTML)
# The town is the hi-fi map on plain grass (make_town.py, run first); Pax comes from Tools/sprites/hifi/. Run from the repo root: sh Tools/sprites/splash/render.sh
set -e
CHROME="/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"
DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$DIR/../../.."
render() {
  "$CHROME" --headless=new --disable-gpu --hide-scrollbars --allow-file-access-from-files --default-background-color=00000000 \
    --window-size=${3:-1920,1080} --virtual-time-budget=5000 --screenshot="$2" "file://$DIR/$1.html" 2>/dev/null
}
python3 "$DIR/make_town.py"
render splash "$ROOT/Assets/_Project/Art/Splash/byte_world_splash.png"
render backdrop "$ROOT/Assets/_Project/Resources/Sprites/UI/title_backdrop.png"
render logo "$ROOT/Assets/_Project/Resources/Sprites/UI/logo.png" 1600,260
python3 "$DIR/make_intro.py"
for n in 1 2 3; do render "intro_$n" "$ROOT/Assets/_Project/Resources/Sprites/UI/intro_$n.png" 960,540; done
echo "splash art written"
