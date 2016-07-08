SOURCES = $(wildcard *.cs)

all: mkdir_preprossed clean compile

mkdir_preprossed:
	mkdir -p preprocessed

preprocessed/%.cs: %.cs 
	cpp -P $^ $@

compile: $(foreach var, ${SOURCES}, preprocessed/${var})
	mcs -d:DEBUG -debug -out:run.exe $^

clean:
	rm -rf preprocessed/*.cs
