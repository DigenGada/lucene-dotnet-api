## Project Roadmap ##
The Hammer (which is the codename for this library) is still in its infancy. I've been developing it for the past couple of months and in that time its been built, torn down, and refactored a couple of times. The more I use it and hear people using the more I want to change it to make it better. The current version of the source code, as of 2/14/12, is somewhat fragmented but is stable. The code will be going through another major refactor over the new few weeks so be sure to check in with this roadmap from time to time to see where we stand.

## Existing Functionality ##
The current source code includes:
  * Basic, yet extendable, analytics engine
  * Basic file parsing classes for parsing office files, pdfs, et cetera
  * An additional console application for feature testing
  * Compression helpers for compressing data before indexing
  * Phonetics helpers for doing 'sounds-like' matching
  * Index writer extensions that allow users to write anything from primitives to complex types with ease (seriously, you can probably do it with a one liner)
  * A barebones sql query parser

## Current Development Phase ##
Stuff that I'm working on right now:
  * A large code refactor that's likely going to change the structure of how most of the objects work together. For most purposes you should not consider the current release and the next phase release compatible with each other.
  * XML helpers to allow easy indexing and retrieving of xml style data
  * Flush out the document parser helpers so that they're either fully stable complete versions of parsers or removed
  * Flush out the web namespace so that it either provides excellent support for webpage indexing and crawling or is removed
  * Flush out the SQL query parser and either put it to good use or remove it
  * Add support for virtual indexes

## Future Enhancements ##
  * Create unit tests that cover the majority of functionality within the library
  * Update full in-line documentation after refactor (includes updating the wiki and creating how-to videos)
  * Everything the people want!