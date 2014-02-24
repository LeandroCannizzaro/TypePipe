// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;

namespace Remotion.UnitTests.Reflection.TestDomain
{
  public class ClassForSignatureComparisons
  {
    public ClassForSignatureComparisons () { }
    public ClassForSignatureComparisons (int i) { }

    public int M1 () { return 0; }
    public int M2 () { return 0; }
    public int M3 (int i) { return 0; }

    public int P1 { get; set; }
    public int P2 { get; set; }
    public string P3 { get; set; }

    public event EventHandler E1;
    public event EventHandler E2;
    public event EventHandler<EventArgs> E3;

    public string F1;
    public string F2;
    public object F3;
  }
}