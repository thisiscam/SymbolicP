# P translators

A set of translators that translates the P language into basic C# source code(complete), symbolic C# source code(under progress). 

## Get Started

This project uses ANTLR4 as its parser front end; it uses ANTLR4's java runtime to parse P program sources efficiently, after which the parse result is converted to python objects through s-expression, so that the translators are written in python.

The ANTLR4 runtime jar is included in the package, so if you are just running the translator, you should be good with a java runtime. However, if you wish to develope, a full ANTLR parser generator runtime might be needed. 

For running the python translators, 

It's recommanded to use virtualenv:
    
    $ virtualenv env              # run this the first time you setup the code 
    $ source env/bin/activate     # run this everytime you want to run or develop on the project
    
Install pip dependencies:

    $ pip install -r requirements.txt

And when you are done:

    $ deactivate

## Translators

* Currently, only Basic C# Translator is complete. You can try with:

```
    $ python -m scripts.translate -h
```

* Three targets are supported:

    | Target                   | Commandline Option |
    |--------------------------|--------------------|
    | Random explorer          | -t basic_csharp    |
    | Naive Symbolic Execution | -t symbolic_csharp |
    | MultiSE                  | -t multise_csharp  |

For instance

```
    $ python -m scripts.translate -t basic_csharp tests/PingPong.p
```

This wil generate a folder PingPong, filled with translated .cs sources and a .csproj file. You should be able to build the project by opening the .csproj file or with msbuild/xbuild


