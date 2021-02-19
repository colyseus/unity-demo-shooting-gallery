namespace Colyseus
{
    using System;

    /// <summary>
    ///     Serializable class containing a true/false response, used for generic Requests to server where we just need to know
    ///     if it succeeded or not
    /// </summary>
    [Serializable]
    public class CSAStatusData
    {
        /// <summary>
        ///     The status of this response
        /// </summary>
        public bool Status = false;
    }
}