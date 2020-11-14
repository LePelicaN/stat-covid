module Fable.Import.Chartjs

open Fable.Core.JsInterop
open Fable.Core
open Fable.React

[<RequireQualifiedAccess>]
type DatasetProps =
  | Label of string
  // TODO: change to obj ?
  | Data of int array
  // TODO: proposer la creation du rgba... ?
  | BackgroundColor of string array
  // TODO: proposer la creation du rgba... ?
  | BorderColor of string array
  | BorderWidth of int

[<RequireQualifiedAccess>]
type DataProps =
  | Labels of string array

  static member Datasets (datasets : (DatasetProps seq) array) : DataProps =
    let datas =
      datasets
      |> Array.map (fun v ->
        keyValueList CaseRules.LowerFirst v
      )

    unbox ("datasets", datas)

[<RequireQualifiedAccess>]
type TicksProps =
  | BeginAtZero of bool

[<RequireQualifiedAccess>]
type AxesProps =
  static member Ticks (ticks : TicksProps seq) : AxesProps =
    unbox ("ticks", keyValueList CaseRules.LowerFirst ticks)

[<RequireQualifiedAccess>]
type ScalesProps =
  static member YAxes (yAxesList : (AxesProps seq) array) : ScalesProps =
    let yAxes =
      yAxesList
      |> Array.map (fun v ->
        keyValueList CaseRules.LowerFirst v // Transform the list of props into a JavaScript object
      )

    unbox ("yAxes", yAxes)

[<RequireQualifiedAccess>]
type OptionsProps =
  static member Scales (scales : ScalesProps seq) : OptionsProps =
    unbox ("scales", keyValueList CaseRules.LowerFirst scales)

[<RequireQualifiedAccess>]
type ChartProps =
  static member Data (datas : DataProps seq) : ChartProps =
      unbox ("data", keyValueList CaseRules.LowerFirst datas)

  static member Options (options : OptionsProps seq) : ChartProps =
      unbox ("options", keyValueList CaseRules.LowerFirst options)

let inline processBar (props: ChartProps list) : ReactElement =
  ofImport "Bar" "../../node_modules/react-chartjs-2/dist/react-chartjs-2.js" (keyValueList CaseRules.LowerFirst props) []

let testChartProps =
   processBar
    [
      ChartProps.Data
        [
          DataProps.Labels [| "Red"; "Blue"; "Yellow"; "Green"; "Purple"; "Orange" |]
          DataProps.Datasets
            [|
              [
                DatasetProps.Label "# of Votes"
                DatasetProps.Data [| 12; 19; 3; 5; 2; 3 |]
                DatasetProps.BackgroundColor
                  [|
                    "rgba(255, 99, 132, 0.2)";
                    "rgba(54, 162, 235, 0.2)";
                    "rgba(255, 206, 86, 0.2)";
                    "rgba(75, 192, 192, 0.2)";
                    "rgba(153, 102, 255, 0.2)";
                    "rgba(255, 159, 64, 0.2)"
                  |]
                DatasetProps.BorderColor
                  [|
                    "rgba(255, 99, 132, 1)";
                    "rgba(54, 162, 235, 1)";
                    "rgba(255, 206, 86, 1)";
                    "rgba(75, 192, 192, 1)";
                    "rgba(153, 102, 255, 1)";
                    "rgba(255, 159, 64, 1)"
                  |]
                DatasetProps.BorderWidth 1
              ]
            |]
        ]
      ChartProps.Options
        [
          OptionsProps.Scales
            [
              ScalesProps.YAxes
                [|
                  [
                    AxesProps.Ticks
                      [
                        TicksProps.BeginAtZero true
                      ]
                  ]
                |]
            ]
        ]
    ]