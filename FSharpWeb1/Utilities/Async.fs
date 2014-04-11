namespace Utilities
open System.Threading.Tasks

(*
  These helper functions are from: 
  http://theburningmonk.com/2012/10/f-helper-functions-to-convert-between-asyncunit-and-task/
*)

[<AutoOpen>]
module Async =
    /// Returns an asynchronous computation that waits for the given task to complete.
    let inline awaitPlainTask (task: Task) = 
        // rethrow exception from preceding task if it fauled
        let continuation (t : Task) : unit =
            match t.IsFaulted with
            | true -> raise t.Exception
            | arg -> ()
        task.ContinueWith continuation |> Async.AwaitTask
    
    /// Executes a computation in the thread pool. Returns a Task that has no return value.
    let inline startAsPlainTask (work : Async<unit>) = 
        Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)

