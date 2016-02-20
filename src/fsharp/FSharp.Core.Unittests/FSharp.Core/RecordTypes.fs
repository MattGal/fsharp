﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
module FSharp.Core.Unittests.FSharp_Core.Microsoft_FSharp_Core.RecordTypes

open System
open System.Numerics
open FSharp.Core.Unittests.LibraryTestFx
open NUnit.Framework
open FsCheck
open FsCheck.PropOperators

type Record =
    {   A: int
        B: int
    }


let [<Test>] ``can compare records`` () = 
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        i1 <> i2 ==>
            let r1 = { A = i1; B = i2 }
            let r2 = { A = i1; B = i2 }
            (r1 = r2)                   |@ "r1 = r2" .&.
            ({ r1 with A = r1.B} <> r2) |@ "{r1 with A = r1.B} <> r2" .&.
            (r1.Equals r2)              |@ "r1.Equals r2"

[<Struct>]
type StructRecord =
    {   C: int
        D: int
    }

let [<Test>] ``can compare struct records`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        i1 <> i2 ==>
            let sr1 = { C = i1; D = i2 }
            let sr2 = { C = i1; D = i2 }
            (sr1 = sr2)                    |@ "sr1 = sr2" .&.
            ({ sr1 with C = sr1.D} <> sr2) |@ "{sr1 with C = sr1.D} <> sr2" .&.
            (sr1.Equals sr2)               |@ "sr1.Equals sr2"


let [<Test>] ``pattern matching on struct records`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        let sr1 = { C = i1; D = i2 }
        (match sr1 with
        | { C = c; D = d } when c = i1 && d = i2 -> true
        | _ -> false) 
        |@ "with pattern match on struct record" .&.
        (sr1 |> function 
        | { C = c; D = d } when c = i1 && d = i2 -> true
        | _ -> false)
        |@ "function pattern match on struct record"


let [<Test>] ``let binds using struct records`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        let sr1 = { C = i1; D = i2 }
        let { C = c1; D = d2 } as sr2 = sr1
        (sr1 = sr2)          |@ "sr1 = sr2" .&.
        (c1 = i1 && d2 = i2) |@ "c1 = i1 && d2 = i2"


let [<Test>] ``function argument bindings using struct records`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        let sr1 = { C = i1; D = i2 }
        let test sr1 ({ C = c1; D = d2 } as sr2) =
            sr1 = sr2 && c1 = i1 && d2 = i2
        test sr1 sr1      
        
        
[<Struct>]
type MutableStructRecord =
    {   mutable M1: int
        mutable M2: int
    }                  
    
let [<Test>] ``can mutate struct record fields`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) (m1:int) (m2:int) ->
        (i1 <> m1 && i2 <> m2) ==>
            let mutable sr1 = { M1 = i1; M2 = i2}
            sr1.M1 <- m1
            sr1.M2 <- m2
            sr1.M1 = m1 && sr1.M2 = m2

[<Struct>]
type StructRecordDefaultValue =
    {   [<DefaultValue (false)>] R1: Record
        R2: StructRecord
    }

let [<Test>] ``correct behaviour with a [<DefaultValue>] on a ref type field of a struct record`` () =
    Check.QuickThrowOnFailure <|
        fun (i1:int) (i2:int) ->
            let s = { C = i1; D = i2 }
            let r1 =
                {
                    R2 = s
                }

            (obj.ReferenceEquals (r1.R1, null))     |@ "r1.R1 is null" .&.
            (r1.R2 = { C = i1; D = i2 })            |@ "r1.R2 = { C = i1; D = i2 }"

[<Struct>]
type StructRecordDefaultValue2 =
    {   R1: Record
        [<DefaultValue (false)>] R2: StructRecord
    }

let [<Test>] ``correct behaviour with a [<DefaultValue>] on a value type field of a struct record`` () =
    Check.QuickThrowOnFailure <|
        fun (i1:int) (i2:int) ->
            let r = { A = i1; B = i2 }
            let r1 =
                {
                    R1 = r
                }

            (r1.R1 = { A = i1; B = i2 })    |@ "r1.R1 = { A = i1; B = i2 }" .&.
            (r1.R2 = { C = 0; D = 0 })      |@ "r1.R2 = { C = 0; D = 0 }"

let [<Test>] ``correct behaviour for Unchecked.defaultof on a struct record`` () =
    let x1 = { C = 0; D = 0 }
    let x2 : StructRecordDefaultValue = { R2 = { C = 0; D = 0 } }
    let x3 : StructRecordDefaultValue2 = { R1 = Unchecked.defaultof<Record> }

    let y1 = Unchecked.defaultof<StructRecord>
    let y2 = Unchecked.defaultof<StructRecordDefaultValue>
    let y3 = Unchecked.defaultof<StructRecordDefaultValue2>

    Assert.IsTrue ((x1 = y1))

    Assert.IsTrue (( (obj.ReferenceEquals (x2.R1, null)) = (obj.ReferenceEquals (y2.R1, null)) ))
    Assert.IsTrue ((x2.R2 = x1 ))
    Assert.IsTrue ((y2.R2 = x1 ))

    Assert.IsTrue (( (obj.ReferenceEquals (x3.R1, null)) = (obj.ReferenceEquals (y3.R1, null)) ))
    Assert.IsTrue ((x3.R2 = x1 ))
    Assert.IsTrue ((y3.R2 = x1 ))
 