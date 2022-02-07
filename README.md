# ms
My Sort. A C# app to sort 8-bit text files

Build using your favorite version of .net:

    c:\windows\microsoft.net\framework64\v4.0.30319\csc.exe /debug+ /nologo /o+ /nowarn:0168 ms.cs

Usage

    Usage: ms [-c:X] [-i] [-l] [-n] [-r] [-s:X] [-u] [-w:X] inputfile outputfile
      My Sort
      arguments: inputfile   The file to sort. Text file with 8-bit ascii characters.
                 outputfile  The resulting sorted file. Any existing file will be obliterated.
                 -c:X        Sort starting on column X (0-based)
                 -i          Ignore case when sorting strings
                 -l          Sort based on line length only
                 -n          Sort numbers, not characters
                 -r          Reverse the sort (high to low)
                 -s:X        Line Separators on output: W (Windows CRLF. Default), M (Mac CR), U (Unix LF)
                 -u          Uniquify the output; remove duplicate adjacent lines
                 -w:X        Only include words of length X in the output
      examples:  ms in.txt out.txt
                 ms -c:12 in.txt out.txt
                 ms -w:U -l -r in.txt out.txt
