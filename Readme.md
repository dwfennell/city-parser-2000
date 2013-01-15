City Parser 2000
================

### By Dustin Fennell

****

## Overview

City Parser 2000 is a simple C# program which represents some of the data from binary Sim City 2000&copy; files (.SC2 file format) as C# objects. 

This is part of a larger project I am working on which will facilitate custom comparisons of simulated cities using C# ASP.net. 

## Current Status

The current version of the parser is stable with almost all binary data represented in the City object.

## Future Development

- Interpret more of the SC2 file, particularly the MISC segment.
- ParseBinaryFile will likely take a more complex data structure as input (as opposed to a local filepath). 
- Modifications for greater command line ease-of-use (city filepath input, City object 'ToString()' output).
- Creation of a light-weight version for use in a web application.