# NomaiExplorer

NomaiExplorer is a OWML wrapper for Unity Explorer, it offers the ability for mod developers to execute C# code on the fly and navigate the scenes with a custom inspector.

This mod is meant as a development tool.

## Credits

* [UnityExplorer](https://github.com/sinai-dev/UnityExplorer) written by Sinai.
* [OWML](https://github.com/amazingalek/owml) nebula/alek/Raicuparta for showing me the ways.

### Licensing

This project uses code from:
* (GPL) [ManlyMarco](https://github.com/ManlyMarco)'s [Runtime Unity Editor](https://github.com/ManlyMarco/RuntimeUnityEditor), which I used for some aspects of the C# Console and Auto-Complete features. The snippets I used are indicated with a comment.
* (MIT) [denikson](https://github.com/denikson) (aka Horse)'s [mcs-unity](https://github.com/denikson/mcs-unity). I commented out the `SkipVisibilityExt` constructor since it was causing an exception with the Hook it attempted in IL2CPP.
* (Apache) [InGameCodeEditor](https://assetstore.unity.com/packages/tools/gui/ingame-code-editor-144254) was used as the base for the syntax highlighting for UnityExplorer's C# console, although it has been heavily rewritten and optimized. Used classes are in the `UnityExplorer.CSConsole.Lexer` namespace.
