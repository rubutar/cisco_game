#!/usr/bin/env python3
"""Byte World's chiptune music and sound effects, synthesised in plain Python (no libraries) and written as
22 050 Hz mono 16-bit WAVs to Assets/_Project/Resources/Audio/{Music,Sfx}/. AudioDirector plays them.

Music loops (16 bars each, the echo wraps round so the loop is seamless):
  theme   title and intro, C major, 100 bpm, warm and hopeful
  map     walking the town, G major, 108 bpm, light and bouncy
  shift   station shifts and the siege, A minor, 140 bpm, busy but never scary
  build   build mode, D dorian, 118 bpm, a thinking groove
  peace   a 4-second victory jingle (not a loop)
Sound effects: click, tap, allow, block, correct, wrong, alert, success, fail, collapse, arrive, toolkit,
run, pin, prop.

Run: python3 Tools/audio/make_audio.py"""
import math
import os
import random
import struct

RATE = 22050
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "..", "..", "Assets", "_Project", "Resources", "Audio")

NOTE_INDEX = {"C": 0, "C#": 1, "D": 2, "D#": 3, "E": 4, "F": 5, "F#": 6, "G": 7, "G#": 8, "A": 9, "A#": 10, "B": 11}


def freq(name):
    """'A4' -> 440 Hz."""
    pitch, octave = name[:-1], int(name[-1])
    return 440.0 * 2 ** ((NOTE_INDEX[pitch] + 12 * (octave + 1) - 69) / 12)


# ---------- Oscillators ----------

def square(phase, duty=0.5):
    return 1.0 if (phase % 1.0) < duty else -1.0


def triangle(phase):
    p = phase % 1.0
    return 4 * p - 1 if p < 0.5 else 3 - 4 * p


def saw(phase):
    return 2 * (phase % 1.0) - 1


_noise = random.Random(7)


def noise(_phase=0):
    return _noise.uniform(-1, 1)


# ---------- Rendering into a buffer ----------

class Track:
    def __init__(self, seconds):
        self.buf = [0.0] * int(seconds * RATE)

    def note(self, start, length, hz, wave, volume, attack=0.005, release=0.06, slide=0.0, vibrato=0.0, wrap=False):
        """Adds one note. slide: pitch change in octaves over the note; vibrato: depth in semitones."""
        n0, n = int(start * RATE), int(length * RATE)
        phase = 0.0
        total = len(self.buf)
        for i in range(n):
            t = i / RATE
            env = min(1.0, t / attack) if attack > 0 else 1.0
            left = length - t
            if left < release:
                env *= max(0.0, left / release)
            f = hz * (2 ** (slide * t / max(length, 1e-6)))
            if vibrato:
                f *= 2 ** (vibrato * math.sin(2 * math.pi * 5.5 * t) / 12) if t > 0.12 else 1
            phase += f / RATE
            k = n0 + i
            if wrap:
                k %= total
            elif k >= total:
                break
            self.buf[k] += wave(phase) * volume * env

    def echo(self, delay, gain, wrap=True):
        d = int(delay * RATE)
        src = self.buf[:]
        total = len(src)
        for i in range(total):
            j = i + d
            if j >= total:
                if not wrap:
                    break
                j %= total
            self.buf[j] += src[i] * gain

    def write(self, path, peak=0.85):
        top = max(1e-9, max(abs(s) for s in self.buf))
        scale = min(1.0, peak / top)
        frames = b"".join(struct.pack("<h", int(max(-1, min(1, s * scale)) * 32767)) for s in self.buf)
        os.makedirs(os.path.dirname(path), exist_ok=True)
        with open(path, "wb") as f:
            f.write(b"RIFF" + struct.pack("<I", 36 + len(frames)) + b"WAVE")
            f.write(b"fmt " + struct.pack("<IHHIIHH", 16, 1, 1, RATE, RATE * 2, 2, 16))
            f.write(b"data" + struct.pack("<I", len(frames)) + frames)


# ---------- Drums ----------

def kick(track, start, volume=0.9):
    track.note(start, 0.16, 120, lambda p: math.sin(2 * math.pi * p), volume, attack=0.001, release=0.1, slide=-1.6)


def snare(track, start, volume=0.35):
    track.note(start, 0.12, 1, noise, volume, attack=0.001, release=0.1)
    track.note(start, 0.08, 220, triangle, volume * 0.6, attack=0.001, release=0.06, slide=-0.5)


def hat(track, start, volume=0.12, open_=False):
    track.note(start, 0.09 if open_ else 0.035, 1, noise, volume, attack=0.001, release=0.03)


# ---------- A song from bars ----------

def melody_events(text, beat):
    """'E5:2 G5:2 -:1' -> [(offset, length, note)], durations in eighth notes."""
    out, t = [], 0.0
    for token in text.split():
        name, eighths = token.split(":")
        length = int(eighths) * beat / 2
        if name != "-":
            out.append((t, length, name))
        t += length
    return out, t


def song(bpm, chords, melody_bars, lead, bass_style, drums, arp=None, lead_volume=0.22, echo=True):
    """chords: one chord per bar, each a list of note names (root first). melody_bars: one string per bar."""
    beat = 60.0 / bpm
    bar = beat * 4
    track = Track(bar * len(chords))
    for b, chord in enumerate(chords):
        t0 = b * bar
        root = chord[0]
        bass_note = root[:-1] + str(int(root[-1]) - 1) if int(root[-1]) > 2 else root
        # Bass
        if bass_style == "pulse":          # root on every eighth
            for i in range(8):
                track.note(t0 + i * beat / 2, beat / 2 * 0.8, freq(bass_note), triangle, 0.32)
        elif bass_style == "walk":         # root, fifth, octave, fifth
            fifth = chord[2]
            fifth_low = fifth[:-1] + str(int(fifth[-1]) - 1)
            for i, n in enumerate((bass_note, fifth_low, bass_note, fifth_low)):
                track.note(t0 + i * beat, beat * 0.85, freq(n), triangle, 0.34)
        else:                              # "long": two halves
            for i in range(2):
                track.note(t0 + i * beat * 2, beat * 1.8, freq(bass_note), triangle, 0.32)
        # Arpeggio
        if arp:
            notes = chord + [chord[1][:-1] + str(int(chord[1][-1]) + 1)]
            steps = 8 if arp == "eighths" else 16
            for i in range(steps):
                n = notes[i % len(notes)]
                step = bar / steps
                track.note(t0 + i * step, step * 0.7, freq(n), lambda p: square(p, 0.125), 0.055, release=0.03)
        # Drums
        for i in range(8):
            t = t0 + i * beat / 2
            if drums == "soft":
                if i in (0, 4):
                    kick(track, t, 0.5)
                if i % 2 == 1:
                    hat(track, t, 0.06)
            elif drums == "drive":
                if i % 2 == 0:
                    kick(track, t, 0.8)
                if i in (2, 6):
                    snare(track, t, 0.32)
                hat(track, t, 0.09)
            elif drums == "groove":
                if i in (0, 3, 4):
                    kick(track, t, 0.75)
                if i in (2, 6):
                    snare(track, t, 0.28)
                hat(track, t, 0.07, open_=(i == 7))
            elif drums == "hats":
                hat(track, t, 0.05 if i % 2 else 0.08)
        # Melody
        events, _ = melody_events(melody_bars[b % len(melody_bars)], beat)
        for off, length, name in events:
            track.note(t0 + off, length * 0.92, freq(name), lead, lead_volume, release=0.05, vibrato=0.25)
    if echo:
        track.echo(beat * 0.75, 0.22)
    return track


# ---------- The music ----------

def theme():
    c, g, am, f = ["C4", "E4", "G4"], ["G3", "B3", "D4"], ["A3", "C4", "E4"], ["F3", "A3", "C4"]
    chords = [c, g, am, f] * 4
    a = ["E5:2 G5:2 C6:3 B5:1", "D6:2 B5:2 G5:4", "A5:2 C6:2 E6:3 D6:1", "C6:2 A5:2 F5:4",
         "E5:1 F5:1 G5:2 E5:2 C5:2", "D5:2 G5:2 B5:2 D6:2", "C6:3 B5:1 A5:2 E5:2", "F5:2 A5:2 G5:4"]
    b = ["G5:1 G5:1 C6:2 G5:1 E5:1 G5:2", "B5:1 B5:1 D6:2 B5:1 G5:1 D6:2", "C6:1 C6:1 E6:2 C6:1 A5:1 E6:2",
         "F6:2 E6:2 D6:2 C6:2", "E6:3 D6:1 C6:2 G5:2", "B5:3 A5:1 G5:2 D5:2", "E5:2 A5:2 C6:2 B5:2",
         "A5:2 G5:2 F5:2 G5:2"]
    return song(100, chords, a + b, lambda p: square(p, 0.25), "long", "soft", arp="eighths")


def map_music():
    g, em, c, d = ["G3", "B3", "D4"], ["E3", "G3", "B3"], ["C4", "E4", "G4"], ["D4", "F#4", "A4"]
    chords = [g, em, c, d] * 4
    mel = ["B5:2 D6:2 -:1 B5:1 G5:2", "E5:2 G5:2 B5:4", "C6:1 B5:1 A5:2 G5:2 E5:2", "F#5:2 A5:2 D6:4",
           "G5:1 A5:1 B5:2 D6:2 B5:2", "G5:2 E5:2 -:2 B4:2", "C5:2 E5:2 G5:2 C6:2", "A5:3 G5:1 F#5:2 A5:2",
           "-:2 D6:1 D6:1 B5:2 G5:2", "-:2 E6:1 D6:1 B5:2 G5:2", "-:2 C6:1 C6:1 E6:2 C6:2", "D6:2 C6:2 B5:2 A5:2",
           "B5:3 G5:1 D5:2 G5:2", "E5:3 G5:1 B5:4", "C6:2 B5:2 A5:2 E5:2", "F#5:2 E5:2 D5:2 F#5:2"]
    return song(108, chords, mel, triangle, "walk", "hats", arp="sixteenths", lead_volume=0.3)


def shift():
    am, f, c, g = ["A3", "C4", "E4"], ["F3", "A3", "C4"], ["C4", "E4", "G4"], ["G3", "B3", "D4"]
    chords = [am, f, c, g] * 4
    mel = ["A5:1 -:1 A5:1 C6:1 E6:2 D6:1 C6:1", "A5:1 -:1 A5:1 C6:1 F6:2 E6:1 C6:1",
           "G5:1 -:1 G5:1 C6:1 E6:2 D6:1 C6:1", "B5:1 -:1 B5:1 D6:1 G6:2 D6:2",
           "A5:1 -:1 A5:1 C6:1 E6:2 D6:1 C6:1", "A5:1 -:1 A5:1 C6:1 F6:2 E6:1 C6:1",
           "G5:1 -:1 G5:1 C6:1 E6:2 G6:2", "E6:2 D6:2 B5:2 G5:2",
           "E5:2 A5:2 C6:2 A5:2", "F5:2 A5:2 C6:2 F6:2", "E6:1 D6:1 C6:2 G5:2 C6:2", "D6:4 B5:4",
           "E6:2 C6:2 A5:2 E5:2", "F5:2 A5:2 C6:2 E6:2", "G6:2 E6:2 C6:2 E6:2", "D6:2 B5:2 G5:2 B5:2"]
    return song(140, chords, mel, lambda p: square(p, 0.5), "pulse", "drive", lead_volume=0.17)


def build():
    dm, g, c, am = ["D4", "F4", "A4"], ["G3", "B3", "D4"], ["C4", "E4", "G4"], ["A3", "C4", "E4"]
    chords = [dm, g, c, am] * 4
    mel = ["-:4 A5:1 C6:1 D6:2", "-:4 B5:1 D6:1 G5:2", "-:4 G5:1 C6:1 E6:2", "-:2 E6:1 D6:1 C6:2 A5:2",
           "-:4 F5:1 A5:1 D6:2", "-:4 D6:1 B5:1 G5:2", "-:2 C6:2 E6:2 G6:2", "E6:3 C6:1 A5:4",
           "D6:2 -:2 F6:2 E6:2", "D6:2 B5:2 -:2 G5:2", "E6:2 -:2 G6:2 E6:2", "C6:2 A5:2 -:2 E5:2",
           "F5:1 A5:1 D6:2 F6:2 D6:2", "G5:1 B5:1 D6:2 G6:2 D6:2", "G5:1 C6:1 E6:2 G6:2 E6:2", "A5:2 C6:2 E6:4"]
    return song(118, chords, mel, lambda p: square(p, 0.25), "walk", "groove", arp="sixteenths", lead_volume=0.18)


def peace_jingle():
    beat = 60 / 132
    t = Track(beat * 9)
    mel = [("C5", 0, 0.5), ("E5", 0.5, 0.5), ("G5", 1, 0.5), ("C6", 1.5, 1.5), ("B5", 3, 0.5), ("C6", 3.5, 0.5),
           ("D6", 4, 0.5), ("E6", 4.5, 3.5)]
    for name, at, length in mel:
        t.note(at * beat, length * beat, freq(name), lambda p: square(p, 0.25), 0.25, release=0.2, vibrato=0.3)
    for name in ("C4", "E4", "G4"):
        t.note(4.5 * beat, 3.5 * beat, freq(name), triangle, 0.16, release=0.6)
    for at in (0, 1, 2, 3, 4.5):
        kick(t, at * beat, 0.5)
    t.echo(beat * 0.75, 0.2, wrap=False)
    return t


# ---------- Sound effects ----------

def sfx(seconds, *notes, echo=None):
    """notes: (start, length, note or Hz, wave, volume, extra kwargs)."""
    t = Track(seconds)
    for start, length, pitch, wave, volume, *extra in notes:
        hz = freq(pitch) if isinstance(pitch, str) else pitch
        t.note(start, length, hz, wave, volume, **(extra[0] if extra else {}))
    if echo:
        t.echo(echo[0], echo[1], wrap=False)
    return t


def sound_effects():
    sq = lambda p: square(p, 0.5)
    sq25 = lambda p: square(p, 0.25)
    return {
        "click": sfx(0.06, (0, 0.04, 1300, sq25, 0.5, {"release": 0.02})),
        "tap": sfx(0.06, (0, 0.04, 820, triangle, 0.7, {"release": 0.03})),
        "allow": sfx(0.3, (0, 0.08, "C6", sq25, 0.45), (0.08, 0.18, "G6", sq25, 0.45, {"release": 0.12})),
        "block": sfx(0.25, (0, 0.18, 180, sq, 0.5, {"slide": -1.0, "release": 0.1}), (0, 0.06, 1, noise, 0.4)),
        "correct": sfx(0.4, (0, 0.07, "C6", sq25, 0.4), (0.07, 0.07, "E6", sq25, 0.4),
                       (0.14, 0.2, "G6", sq25, 0.4, {"release": 0.15}), echo=(0.09, 0.25)),
        "wrong": sfx(0.35, (0, 0.13, "E4", sq, 0.35), (0.14, 0.2, "C4", sq, 0.35, {"release": 0.12}),
                     (0, 0.34, "E3", saw, 0.12)),
        "alert": sfx(0.75, (0, 0.17, "A5", sq25, 0.35), (0.19, 0.17, "E5", sq25, 0.35),
                     (0.38, 0.17, "A5", sq25, 0.35), (0.57, 0.17, "E5", sq25, 0.35)),
        "success": sfx(0.9, (0, 0.1, "C5", sq25, 0.35), (0.1, 0.1, "E5", sq25, 0.35), (0.2, 0.1, "G5", sq25, 0.35),
                       (0.3, 0.5, "C6", sq25, 0.38, {"release": 0.3, "vibrato": 0.3}),
                       (0.3, 0.5, "C4", triangle, 0.3, {"release": 0.3}), echo=(0.12, 0.25)),
        "fail": sfx(0.8, (0, 0.6, "G4", sq, 0.32, {"slide": -1.2, "release": 0.3}),
                    (0, 0.6, "G3", triangle, 0.3, {"slide": -1.2, "release": 0.3})),
        "collapse": sfx(1.6, (0, 1.4, "C5", saw, 0.22, {"slide": -2.5, "release": 0.6}),
                        (0, 1.4, 1, noise, 0.12, {"release": 1.0}), (0.2, 1.2, "C3", triangle, 0.35, {"slide": -1.0})),
        "arrive": sfx(0.25, (0, 0.06, "G5", triangle, 0.5), (0.07, 0.12, "D6", triangle, 0.5, {"release": 0.08})),
        "toolkit": sfx(1.0, *[(i * 0.06, 0.12, n, sq25, 0.25, {"release": 0.08})
                              for i, n in enumerate(("C6", "E6", "G6", "C7", "G6", "C7", "E7"))],
                       (0.42, 0.5, "C7", triangle, 0.3, {"release": 0.4, "vibrato": 0.4}), echo=(0.1, 0.3)),
        "run": sfx(0.6, (0, 0.55, 1, noise, 0.18, {"attack": 0.2, "release": 0.3}),
                   (0, 0.5, "C5", sq25, 0.12, {"slide": 1.0, "release": 0.2})),
        "pin": sfx(0.45, (0, 0.06, "E6", sq25, 0.4), (0.06, 0.3, "B6", triangle, 0.5, {"release": 0.25}),
                   echo=(0.08, 0.3)),
        "prop": sfx(0.35, (0, 0.08, "D5", triangle, 0.5), (0.1, 0.08, "A5", triangle, 0.5),
                    (0.2, 0.12, "F#5", triangle, 0.45, {"release": 0.08})),
    }


def main():
    music = {"theme": theme(), "map": map_music(), "shift": shift(), "build": build(), "peace": peace_jingle()}
    for name, track in music.items():
        track.write(os.path.join(OUT, "Music", f"{name}.wav"), peak=0.8)
        print(f"music/{name}.wav  {len(track.buf) / RATE:.1f}s")
    for name, track in sound_effects().items():
        track.write(os.path.join(OUT, "Sfx", f"{name}.wav"), peak=0.85)
    print("sfx written")


if __name__ == "__main__":
    main()
