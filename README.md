MarkdownGenerator
===
Generate markdown from C# binary & xml document for GitHub Wiki.

Sample: See [UniRx/wiki](https://github.com/neuecc/UniRx/wiki)

How to Use
---
Clone and open solution, build console application.

Command Line Argument
- `[0:N]` = dll src path

Put .xml on same directory, use document comment for generate.

for example

```
MarkdownGenerator.exe a.dll b.dll c.dll  ...
```