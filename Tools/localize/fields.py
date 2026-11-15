"""Which JSON string fields are player-facing text (translated) per data file. Everything else (ids, job keys,
URLs, example addresses and IPs, Splunk search syntax, Cisco product names, character names) stays as is."""
TRANSLATE = {
    "build": {"goal", "label", "name", "term", "text"},
    "checkpoints": {"learnMoreLabel", "line", "realWorld", "station", "what", "with", "withLines", "without", "withoutLines"},
    "days": {"body", "hint", "rulesChangeLine", "says", "title", "signature"},
    "dialogue": {"alertLine", "backToWorld", "backupLine", "backups", "backupsLeftFormat", "call", "cards", "collapse",
                 "complaintFailLine", "complaintLine", "complaints", "concept", "cta", "dayAlertFormat", "dayPuzzleFormat",
                 "enter", "explanation", "failBlocked", "failLeak", "failNoPath", "failWrongHouse", "fieldLabels",
                 "fieldNotesFormat", "fixBuild", "goIn", "intro", "introLines", "jobLine", "keepWatching", "keyHint",
                 "letsBuild", "line", "lines", "lockFormat", "next", "options", "outcomes", "pause", "peace", "pin",
                 "place", "play", "playAgain", "primaryAction", "priority", "prompt", "quarantine", "question",
                 "report", "restartDay", "resume", "role", "rotatePhone", "run", "runResultFormat", "secondaryAction",
                 "seeMaterials", "skip", "sleep", "snatchLine", "snatchedFormat", "startAgain", "startDay", "success",
                 "successLine", "tagline", "term", "text", "title", "toolkit", "toolkitCountFormat",
                 "toolkitToastFormat", "tryAgain", "tryLabel", "valueLabels", "waitingLine", "announce", "name"},
    "hunt": {"prompt", "text", "term", "concept", "role"},
    "roaming": {"lines"},
}
FILES = ["dialogue", "days", "checkpoints", "build", "hunt", "roaming"]
