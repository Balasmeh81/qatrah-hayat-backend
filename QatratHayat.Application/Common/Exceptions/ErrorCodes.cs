namespace QatratHayat.Application.Common.Exceptions
{
    public static class ErrorCodes
    {
        // General
        public const string ValidationError = "VALIDATION_ERROR";
        public const string BadRequest = "BAD_REQUEST";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string NotFound = "NOT_FOUND";
        public const string Conflict = "CONFLICT";
        public const string InternalServerError = "INTERNAL_SERVER_ERROR";

        // Auth / Account
        public const string AuthInvalidCredentials = "AUTH_INVALID_CREDENTIALS";
        public const string AuthAccountInactive = "AUTH_ACCOUNT_INACTIVE";
        public const string AuthMissingUserIdClaim = "AUTH_MISSING_USER_ID_CLAIM";
        public const string AuthInvalidUserIdClaim = "AUTH_INVALID_USER_ID_CLAIM";

        public const string RegistrationFailed = "REGISTRATION_FAILED";
        public const string RoleAssignmentFailed = "ROLE_ASSIGNMENT_FAILED";

        public const string NationalIdRequired = "NATIONAL_ID_REQUIRED";
        public const string NationalIdNotFound = "NATIONAL_ID_NOT_FOUND";
        public const string NationalIdAlreadyRegistered = "NATIONAL_ID_ALREADY_REGISTERED";
        public const string EmailAlreadyRegistered = "EMAIL_ALREADY_REGISTERED";
        public const string NonJordanianCitizen = "NON_JORDANIAN_CITIZEN";

        public const string UserNotFound = "USER_NOT_FOUND";
        public const string UserRoleNotAssigned = "USER_ROLE_NOT_ASSIGNED";
        public const string UserRoleInvalid = "USER_ROLE_INVALID";
        public const string DonorProfileNotFound = "DONOR_PROFILE_NOT_FOUND";

        // Screening
        public const string UnsupportedScreeningSessionType = "UNSUPPORTED_SCREENING_SESSION_TYPE";
        public const string DonationIntentRequired = "DONATION_INTENT_REQUIRED";
        public const string DonationIntentMustBeNull = "DONATION_INTENT_MUST_BE_NULL";
        public const string RegistrationAlreadyCompleted = "REGISTRATION_ALREADY_COMPLETED";
        public const string DonorProfileRequired = "DONOR_PROFILE_REQUIRED";
        public const string DonationIntentNotFound = "DONATION_INTENT_NOT_FOUND";
        public const string DonationIntentOwnershipMismatch = "DONATION_INTENT_OWNERSHIP_MISMATCH";

        public const string NoActiveScreeningQuestions = "NO_ACTIVE_SCREENING_QUESTIONS";
        public const string DuplicateQuestionAnswers = "DUPLICATE_QUESTION_ANSWERS";
        public const string InvalidQuestionIds = "INVALID_QUESTION_IDS";
        public const string MissingQuestionAnswers = "MISSING_QUESTION_ANSWERS";
        public const string FemaleOnlyQuestionViolation = "FEMALE_ONLY_QUESTION_VIOLATION";
        public const string DateValueRequired = "DATE_VALUE_REQUIRED";
        public const string DateValueNotAllowed = "DATE_VALUE_NOT_ALLOWED";
        public const string AdditionalTextRequired = "ADDITIONAL_TEXT_REQUIRED";
        public const string AdditionalTextNotAllowed = "ADDITIONAL_TEXT_NOT_ALLOWED";
    }
}