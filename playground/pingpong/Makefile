SOURCES = $(wildcard *.cs)

all: clean compile

preprocessed/%.cs: %.cs 
	cpp -P $^ $@

compile: $(foreach var, ${SOURCES}, preprocessed/${var})
	mcs -debug -out:run.exe $^

clean:
	rm preprocessed/*.cs
