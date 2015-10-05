using SmartStore.PayUMea;

namespace SmartStore.PayUMea.Settings
{
	/// <summary>
    /// Represents payment processor transaction mode
    /// </summary>
    public enum TransactMode : int
    {
        /// <summary>
        /// Authorize
        /// </summary>
        Authorize = 1,
        /// <summary>
        /// Authorize and capture
        /// </summary>
        AuthorizeAndCapture = 2
    }
}
