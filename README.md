# Performance Test LCS - LTSAPI
## What is this project for

This project does a performace comparision for an availability request on two different Interfaces.  
For each interface there are 5 predefined requests (Folder LCS / LTS) where the dates can be changed.


## How to start the application

Create a secret file or use Environment variables and add all values for:  

```javascript
{
  "LTSApi": {
    "serviceurl": "",
    "username": "",
    "password": "",
    "xltsclientid": ""
  },
  "LcsConfig": {
    "username": "",
    "password": "",
    "messagepassword": "",
    "serviceurl": ""
  }
}
```

Configure the dates which should be retrieved in Program.cs (line 19)

```
        List<Tuple<string, string>> datestotest = new List<Tuple<string, string>>()
        {
            Tuple.Create("2025-12-01","2025-12-05"),
            Tuple.Create("2025-12-02","2025-12-03"),
            Tuple.Create("2025-11-20","2025-11-23"),
            Tuple.Create("2025-11-20","2025-11-23"),
            Tuple.Create("2025-11-27","2025-11-28"),
            Tuple.Create("2025-11-24","2025-11-30"),
            Tuple.Create("2025-11-19","2025-11-23"),            
        };
```