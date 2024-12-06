namespace Repositories.Common;

public static class Constant
{
    #region Conversation

    public const int MaxNumberOfMembersInConversation = 2;

    #endregion

    #region Security

    public const int AccessTokenValidityInMinutes = 5;
    public const int RefreshTokenValidityInDays = 7;
    public const int VerificationCodeValidityInMinutes = 15;
    public const int VerificationCodeLength = 6;
    public const int ResetPasswordTokenValidityInMinutes = 15;

    #endregion

    #region Pagination

    // Default
    public const int DefaultMinPageSize = 10;
    public const int DefaultMaxPageSize = 50;

    // Conversation
    public const int ConversationMinPageSize = 20;

    #endregion

    #region Cache

    // Default
    public const int DefaultAbsoluteExpirationInMinutes = 60;
    public const int DefaultSlidingExpirationInMinutes = 30;

    #endregion
}