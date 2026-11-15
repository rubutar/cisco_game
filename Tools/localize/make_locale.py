#!/usr/bin/env python3
"""Builds the Italian data files: every Resources/Data/<file>.json copied to Resources/Data/it/<file>.json with its
player-facing text (fields.py) translated through it.json (English -> Italian). ids, keys, numbers and everything
else stay identical, so both languages run the same game. Fails if a string has no translation or a {0}
placeholder doesn't match; warns about lines over 110 characters.
Run after changing any English text or it.json:  python3 Tools/localize/make_locale.py"""
import json
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from fields import TRANSLATE, FILES  # noqa: E402

DATA = os.path.join(HERE, "..", "..", "Assets", "_Project", "Resources", "Data")


def main(lang="it"):
    table = json.load(open(os.path.join(HERE, f"{lang}.json"), encoding="utf-8"))
    missing, bad = [], []

    def tr(text):
        if not text.strip():
            return text
        if text not in table:
            missing.append(text)
            return text
        out = table[text]
        if sorted(re.findall(r"\{\d\}", out)) != sorted(re.findall(r"\{\d\}", text)):
            bad.append((text, out))
        return out

    def walk(o, f, key=None):
        if isinstance(o, dict):
            return {k: walk(v, f, k) for k, v in o.items()}
        if isinstance(o, list):
            return [walk(v, f, key) for v in o]
        if isinstance(o, str) and key in TRANSLATE[f]:
            return tr(o)
        return o

    os.makedirs(os.path.join(DATA, lang), exist_ok=True)
    for f in FILES:
        src = json.load(open(os.path.join(DATA, f + ".json"), encoding="utf-8"))
        with open(os.path.join(DATA, lang, f + ".json"), "w", encoding="utf-8") as out:
            out.write(json.dumps(walk(src, f), indent=2, ensure_ascii=False) + "\n")

    long_lines = [v for v in table.values() if len(v) > 110]
    for v in long_lines:
        print(f"warning: {len(v)} characters: {v}")
    if missing or bad:
        for m in missing:
            print("missing:", m)
        for t, o in bad:
            print("placeholder mismatch:", t, "->", o)
        sys.exit(1)
    print(f"{lang}: {len(FILES)} files written, {len(table)} strings")


if __name__ == "__main__":
    main(*sys.argv[1:])
