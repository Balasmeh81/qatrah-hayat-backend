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

        // Users Management
        public const string StaffUserNotFound = "STAFF_USER_NOT_FOUND";
        public const string CitizenUserNotFound = "CITIZEN_USER_NOT_FOUND";

        public const string UserIsNotStaff = "USER_IS_NOT_STAFF";
        public const string UserIsNotCitizen = "USER_IS_NOT_CITIZEN";

        public const string InvalidStaffRole = "INVALID_STAFF_ROLE";
        public const string StaffRoleRequired = "STAFF_ROLE_REQUIRED";

        public const string BranchRequiredForStaffRole = "BRANCH_REQUIRED_FOR_STAFF_ROLE";
        public const string HospitalRequiredForDoctor = "HOSPITAL_REQUIRED_FOR_DOCTOR";

        public const string BranchNotFound = "BRANCH_NOT_FOUND";
        public const string HospitalNotFound = "HOSPITAL_NOT_FOUND";

        public const string EmailAlreadyUsed = "EMAIL_ALREADY_USED";
        public const string NationalIdAlreadyUsed = "NATIONAL_ID_ALREADY_USED";
        public const string DeletedUserCannotBePromoted = "DELETED_USER_CANNOT_BE_PROMOTED";

        public const string UserActivationFailed = "USER_ACTIVATION_FAILED";
        public const string UserDeactivationFailed = "USER_DEACTIVATION_FAILED";
        public const string UserDeletionFailed = "USER_DELETION_FAILED";

        public const string CannotDeleteOwnAccount = "CANNOT_DELETE_OWN_ACCOUNT";
        public const string CannotDeactivateOwnAccount = "CANNOT_DEACTIVATE_OWN_ACCOUNT";

        public const string PermanentDeferralReasonRequired = "PERMANENT_DEFERRAL_REASON_REQUIRED";
        public const string DeferralRecordNotFound = "DEFERRAL_RECORD_NOT_FOUND";

        public const string StaffCreationFailed = "STAFF_CREATION_FAILED";
        public const string StaffUpdateFailed = "STAFF_UPDATE_FAILED";
        public const string CitizenUpdateFailed = "CITIZEN_UPDATE_FAILED";

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