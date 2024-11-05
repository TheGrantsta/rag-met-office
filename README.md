# rag-met-office

## Explore using RAG with Met Office API data

Basic console app designed to epxlore the feasibility of using RAG with Met Office weather data as the context. It appears to return useful summaries of the next four hours based on the Met Office hourly spot data

## To run console app locally

You will need to provide your own:

* Met Office Hourly Spot Data API Key - free but limited to 500 requests/day
* OpenAI Api Key
* OpenAI Organisation Id
* OpenAI Project Id

The program requires a valid latitude and longitude to run, and the values in the hint are for Barnet in North London

## User secrets commands

### Initialise
```console
dotnet user-secrets init
```

### List
```console
dotnet user-secrets list
```

### Set
```console
dotnet user-secrets set "<Name>" "<Value>"
```

## Running console app

By default, when running the console app you will be prompted for latitude and longitude, and it will use the Met Office hourly data API. To use the Tomorrow.Io API, pass any string as a command line argument