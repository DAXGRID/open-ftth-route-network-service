using FluentResults;

namespace OpenFTTH.RouteNetwork.API.Commands
{
    public class InterestValidationError : Error
    {
        public InterestValidationErrorCodes Code { get; }
        public InterestValidationError(InterestValidationErrorCodes errorCode, string errorMsg) : base(errorCode.ToString() + ": " + errorMsg)
        {
            this.Code = errorCode;
            Metadata.Add("ErrorCode", errorCode.ToString());
        }
    }
}
