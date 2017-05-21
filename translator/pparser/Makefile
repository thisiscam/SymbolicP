ANTLR4 = java -Xmx500m org.antlr.v4.Tool
GRUN = java org.antlr.v4.runtime.misc.TestRig

TEMPDIR := $(shell mktemp -u)

all: parser_jar

parser_python_visitor: p.g4
	echo "Generating python visitor..."
	$(ANTLR4) -visitor -no-listener -Dlanguage=Python2 -o . $^

parser_src: p.g4
	$(ANTLR4) -no-visitor -no-listener -o java $^

parser_jar: parser_src
	mkdir -p build
	javac -d build/ java/*.java
	jar cfm pparser.jar java/MANIFEST -C build .
	rm -rf build

clean: 
	rm -rf java/* build 
