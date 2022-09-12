# hash-n-chain
Based on Erik Demain's MIT lecture on hashing &amp; chaining - https://www.youtube.com/watch?v=0M_kIqhwbFo

StreamDictionary is a stream based implementation of IDictionary. The idea is to read/write hash maps from/to disk using a FileStream.
Why use IDictionary as the interface for an open-hash file format? Because it's ready-made and easy to implement and can be tested against the .Net Dicionary<,> class.

I wouldn't use this in a production system. It's just a proof of concept for a linked list using stream offsets as pointers.

[![.NET](https://github.com/marklauter/hash-n-chain/actions/workflows/dotnet.yml/badge.svg)](https://github.com/marklauter/hash-n-chain/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/marklauter/hash-n-chain/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/marklauter/hash-n-chain/actions/workflows/codeql-analysis.yml)
