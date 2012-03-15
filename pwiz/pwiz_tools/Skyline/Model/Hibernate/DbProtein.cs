﻿/*
 * Original author: Nick Shulman <nicksh .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2009 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;

namespace pwiz.Skyline.Model.Hibernate
{
    [QueryTable(TableType = TableType.node)]
    public class DbProtein : DbEntity
    {
        public override Type EntityClass
        {
            get { return typeof(DbProtein); }
        }
        [QueryColumn(FullName="ProteinName")]
        public virtual string Name { get; set; }
        [QueryColumn(FullName="ProteinDescription")]
        public virtual string Description { get; set; }
        [QueryColumn(FullName="ProteinSequence")]
        public virtual string Sequence { get; set; }
        [QueryColumn(FullName="ProteinNote")]
        public virtual string Note { get; set; }
    }
}