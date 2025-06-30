Contains uima/ctakes-related ytex components:
* DBCollectionReader - UIMA collection reader to retrieve documents from a database
* DBConsumer - store annotations in a database
* SenseDisambiguatorAnnotator - Disambiguate named entities assigned multiple concepts
* SentenceDetector - just like normal sentence detector, but do not force sentence splits at new lines
* DateAnnotator - convert dates identified by ctakes date annotator into real dates
* MetaMapToCTakesAnnotator - convert metamap annotations into ctakes annotations
* NamedEntityRegexAnnotator - use regular expressions for named entity recognition
* SegmentRegexAnnotator - use regular expressions to identify document sections

Go through the steps outlined in ctakes-ytex, then you'll be good to go for this project.
