# One-off: replace Integration's with CivilisationEngine's in Roadmap.md
path = "Docs/Roadmap.md"
with open(path, "r", encoding="utf-8") as f:
    s = f.read()
# Try both apostrophe variants
for apostrophe in ["'", "\u2019"]:
    old = "either adopt Integration" + apostrophe + "s REGIMES"
    if old in s:
        s = s.replace(old, "either adopt CivilisationEngine's REGIMES")
        break
with open(path, "w", encoding="utf-8") as f:
    f.write(s)
print("Done")
