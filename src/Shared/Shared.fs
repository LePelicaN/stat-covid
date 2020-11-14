namespace Shared

open System

type Stat =
  { Hospitalisation : float
    Reanimation : float
    Return : float
    Death : float }

type CovidStat =
  { Day : DateTime
    NbOnDay : Stat
    NewOnDay : Stat option
    Acceleration : Stat option}

// module CovidStat =
//     let isValid (description: string) =
//         String.IsNullOrWhiteSpace description |> not

//     let create (description: string) =
//         { Id = Guid.NewGuid()
//           Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ICovidStatApi =
    { GetData : string * string -> Async<CovidStat list> }