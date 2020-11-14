namespace Shared

open System

type Stat =
  { Hospitalisation : float
    Reanimation : float
    Return : float
    Death : float }

  static member (/) (lhs: Stat, rhs: float) =
    {
      Hospitalisation = lhs.Hospitalisation / rhs
      Reanimation = lhs.Reanimation / rhs
      Return = lhs.Return / rhs
      Death = lhs.Death / rhs
    }

  static member (-) (lhs: Stat, rhs: Stat) =
    {
      Hospitalisation = lhs.Hospitalisation - rhs.Hospitalisation
      Reanimation = lhs.Reanimation - rhs.Reanimation
      Return = lhs.Return - rhs.Return
      Death = lhs.Death - rhs.Death
    }

type CovidStat =
  { Day : DateTime
    NbOnDay : Stat
    NewOnDay : Stat option
    Acceleration : Stat option}

module CovidStatHelper =

  let getNbOnDay stat =
    stat.NbOnDay
  let getNewOnDay stat =
    stat.NewOnDay.Value
  let getAcceleration stat =
    stat.Acceleration.Value

  let getNewOnDay2 stat =
    stat.NewOnDay
  let getAcceleration2 stat =
    stat.Acceleration

  let getHospitalisation covidStat =
    covidStat.Hospitalisation
  let getReanimation covidStat =
    covidStat.Reanimation
  let getReturn covidStat =
    covidStat.Return
  let getDeath covidStat =
    covidStat.Death

  let nbOn covidStat statGetter =
    statGetter covidStat.NbOnDay
  let newOn covidStat statGetter =
    statGetter covidStat.NewOnDay.Value
  let accOn covidStat statGetter =
    statGetter covidStat.Acceleration.Value

  let diffStatsInt (lhs:Stat) (rhs:Stat) =
    {
      Hospitalisation = lhs.Hospitalisation - rhs.Hospitalisation
      Reanimation = lhs.Reanimation - rhs.Reanimation
      Return = lhs.Return - rhs.Return
      Death = lhs.Death - rhs.Death
    }

  let diffStats getter (lhs:CovidStat) (rhs:CovidStat) =
    diffStatsInt (getter lhs) (getter rhs)

  let diffStats2 getterlhs (lhs:CovidStat) getterrhs (rhs:CovidStat) =
    diffStatsInt (getterlhs lhs) (getterrhs rhs)

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
    { GetData : string * string * int * int -> Async<CovidStat list> }