//-----------------------------------------------------------------------
// <copyright file="jet_convert.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

using EsentLib.Api;

namespace EsentLib.Jet
{
    /// <summary>Conversion options for <see cref="IJetDatabase.CompactDatabase"/>. This feature
    /// was discontinued in Windows Server 2003.</summary>
    [Obsolete("Not available in Windows Server 2003 and up.")]
    public abstract class JET_CONVERT
    {
    }
}
