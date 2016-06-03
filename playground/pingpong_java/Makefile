SOURCES = $(wildcard *.java)

all: mkdir_preprossed clean compile

mkdir_preprossed:
	mkdir -p preprocessed

preprocessed/%.java: %.java 
	cpp -P $^ $@

compile: $(foreach var, ${SOURCES}, preprocessed/${var})
	javac -d preprocessed $^

run: 
	java -cp preprocessed Scheduler

clean:
	rm -rf preprocessed/*.java
