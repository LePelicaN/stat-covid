namespace Shared

open System

type CovidStat =
    { Day : DateTime
      NbHospitalisation : int
      NewHospitalisation : int
      NbReanimation : int
      NewReanimation : int
      NbReturn : int
      NewReturn : int
      NbDeath : int
      NewDeath : int }

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