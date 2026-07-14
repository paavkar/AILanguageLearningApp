# AI Language Learning App

## Tech used

- .NET 10,
- .NET MAUI,
- Semantic Kernel,
- Ollama,
- SQLite.

## Introduction

The goal for this application was to create an application for language learning
that would be more personalised for each individual's needs. The basis of this
is an LLM that creates new exercises for the user in various types.

There exists two distinct use cases for the LLM: lesson creation and answer evaluation.
The user could choose the LLM used for either task (e.g. a smaller model for evaluation).

The application is designed for fully offline usage and as such the desktop
environment (Windows, MacOS) uses Ollama models, and the mobile environment
(Android, iOS) is meant to use ONNX runtime both via Semantic Kernel although
the mobile environment is untested. To further go along offline usage, a local
SQLite database is initialised upon application startup where various data
would be stored (lesson information, user accounts).

**NOTE!** The UI is not developed in the slightest, the current UI is for LLM
testing purposes only.
