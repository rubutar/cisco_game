// Stream B. Bridge from the game to the WebGL page template (Assets/WebGLTemplates/ByteWorld).
mergeInto(LibraryManager.library, {
  // Gives the page the "turn your phone sideways" text from dialogue.json (no game text in the template).
  ByteWorldSetRotateText: function (textPtr) {
    var text = UTF8ToString(textPtr);
    if (typeof window.byteWorldSetRotateText === "function") window.byteWorldSetRotateText(text);
  }
});
