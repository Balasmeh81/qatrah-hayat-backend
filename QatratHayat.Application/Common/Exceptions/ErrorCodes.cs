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
        public const string CurrentUserNotFound = "CURRENT_USER_NOT_FOUND";
        public const string UserNotAllowedToConfirmBloodType =
            "USER_NOT_ALLOWED_TO_CONFIRM_BLOOD_TYPE";

        // Auth
        //Login
        public const string AuthInvalidCredentials = "AUTH_INVALID_CREDENTIALS";
        public const string AuthAccountInactive = "AUTH_ACCOUNT_INACTIVE";
        public const string AuthMissingUserIdClaim = "AUTH_MISSING_USER_ID_CLAIM";
        public const string AuthInvalidUserIdClaim = "AUTH_INVALID_USER_ID_CLAIM";

        //Registration
        public const string RegistrationFailed = "REGISTRATION_FAILED";
        public const string RoleAssignmentFailed = "ROLE_ASSIGNMENT_FAILED";

        //Registration Validations
        public const string NationalIdRequired = "NATIONAL_ID_REQUIRED";
        public const string NationalIdNotFound = "NATIONAL_ID_NOT_FOUND";
        public const string NationalIdAlreadyRegistered = "NATIONAL_ID_ALREADY_REGISTERED";

        public const string NonJordanianCitizen = "NON_JORDANIAN_CITIZEN";
        public const string EmailRequired = "EMAIL_REQUIRED";
        public const string EmailAlreadyRegistered = "EMAIL_ALREADY_REGISTERED";
        public const string PhoneNumberAlreadyRegistered = "PHONE_ALREADY_REGISTERED";
        public const string PhoneNumberRequired = "PHONE_NUMBER_REQUIRED";
        public const string InvalidPhoneNumber = "INVALID_PHONE_NUMBER";
        public const string PasswordRequired = "PASSWORD_REQUIRED";
        public const string ConfirmPasswordRequired = "CONFIRM_PASSWORD_REQUIRED";
        public const string PasswordConfirmationMismatch = "PASSWORD_CONFIRMATION_MISMATCH";
        public const string TermsAndConditionsRequired = "TERMS_AND_CONDITIONS_REQUIRED";

        //Forgot Password Errors
        public const string InvalidOtp = "INVALID_OTP";
        public const string OtpExpired = "OTP_EXPIRED";
        public const string OtpTooManyAttempts = "OTP_TOO_MANY_ATTEMPTS";
        public const string InvalidPasswordResetRequest = "INVALID_PASSWORD_RESET_REQUEST";
        public const string PasswordResetSessionExpired = "PASSWORD_RESET_SESSION_EXPIRED";
        public const string PasswordResetFailed = "PASSWORD_RESET_FAILED";
        public const string EmailSendingFailed = "EMAIL_SENDING_FAILED";

        //--------------------------------------------------------------

        //User
        //User Errors
        public const string UserNotFound = "USER_NOT_FOUND";
        public const string UserRoleNotAssigned = "USER_ROLE_NOT_ASSIGNED";
        public const string UserRoleInvalid = "USER_ROLE_INVALID";
        public const string DonorProfileNotFound = "DONOR_PROFILE_NOT_FOUND";

        //--------------------------------------------------------------

        // Users Management
        public const string StaffUserNotFound = "STAFF_USER_NOT_FOUND";
        public const string CitizenUserNotFound = "CITIZEN_USER_NOT_FOUND";
        public const string UserAlreadyExists = "USER_ALREADY_EXISTS";
        public const string UserAlreadyStaff = "USER_ALREADY_STAFF";

        public const string UserIsNotStaff = "USER_IS_NOT_STAFF";
        public const string UserIsNotCitizen = "USER_IS_NOT_CITIZEN";

        public const string InvalidStaffRole = "INVALID_STAFF_ROLE";
        public const string StaffRoleRequired = "STAFF_ROLE_REQUIRED";

        public const string BranchRequiredForStaffRole = "BRANCH_REQUIRED_FOR_STAFF_ROLE";
        public const string HospitalRequiredForDoctor = "HOSPITAL_REQUIRED_FOR_DOCTOR";

        public const string HospitalNotFound = "HOSPITAL_NOT_FOUND";

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

        public const string BranchNotAllowedForDoctor = "BRANCH_NOT_ALLOWED_FOR_DOCTOR";
        public const string HospitalNotAllowedForEmployee = "HOSPITAL_NOT_ALLOWED_FOR_EMPLOYEE";
        public const string BranchManagerAssignmentMustBeManagedFromBranchManagement =
            "BRANCH_MANAGER_ASSIGNMENT_MUST_BE_MANAGED_FROM_BRANCH_MANAGEMENT";
        public const string HospitalNotAllowedForBranchManager =
            "HOSPITAL_NOT_ALLOWED_FOR_BRANCH_MANAGER";
        public const string LocationNotAllowedForAdmin = "LOCATION_NOT_ALLOWED_FOR_ADMIN";

        //--------------------------------------------------------------

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
        public const string RegistrationScreeningRequired = "REGISTRATION_SCREENING_REQUIRED";

        //--------------------------------------------------------------

        // Branch Management
        public const string BranchNotFound = "BRANCH_NOT_FOUND";
        public const string BranchAlreadyExists = "BRANCH_ALREADY_EXISTS";

        public const string BranchManagerNotFound = "BRANCH_MANAGER_NOT_FOUND";
        public const string UserIsNotBranchManager = "USER_IS_NOT_BRANCH_MANAGER";
        public const string BranchManagerAlreadyAssigned = "BRANCH_MANAGER_ALREADY_ASSIGNED";
        public const string UserAlreadyAssignedToHospital = "USER_ALREADY_ASSIGNED_TO_HOSPITAL";
        public const string BranchCreationFailed = "BRANCH_CREATION_FAILED";
        public const string BranchUpdateFailed = "BRANCH_UPDATE_FAILED";
        public const string BranchHasLinkedHospitals = "BRANCH_HAS_LINKED_HOSPITALS";

        public const string InvalidBranchWorkingHours = "INVALID_BRANCH_WORKING_HOURS";
        public const string DuplicateBranchWorkingDay = "DUPLICATE_BRANCH_WORKING_DAY";
        public const string InvalidBranchWorkingTime = "INVALID_BRANCH_WORKING_TIME";

        //--------------------------------------------------------------

        // Hospital Management
        public const string HospitalAlreadyExists = "HOSPITAL_ALREADY_EXISTS";
        public const string BranchInactiveOrNotFound = "BRANCH_INACTIVE_OR_NOT_FOUND";
        public const string HospitalHasLinkedBloodRequests = "HOSPITAL_HAS_LINKED_BLOOD_REQUESTS";

        //--------------------------------------------------------------

        // Blood Request
        public const string BloodRequestNotFound = "BLOOD_REQUEST_NOT_FOUND";
        public const string InvalidBloodRequestStatus = "INVALID_BLOOD_REQUEST_STATUS";
        public const string BloodRequestCreationFailed = "BLOOD_REQUEST_CREATION_FAILED";

        public const string BeneficiaryNotFound = "BENEFICIARY_NOT_FOUND";
        public const string BeneficiaryRequired = "BENEFICIARY_REQUIRED";
        public const string InvalidBeneficiaryNationalId = "INVALID_BENEFICIARY_NATIONAL_ID";

        public const string DoctorNotFoundForHospital = "DOCTOR_NOT_FOUND_FOR_HOSPITAL";
        public const string DoctorNotAssignedToRequest = "DOCTOR_NOT_ASSIGNED_TO_REQUEST";
        public const string DoctorCannotReviewRequest = "DOCTOR_CANNOT_REVIEW_REQUEST";

        public const string EmployeeBranchNotAssigned = "EMPLOYEE_BRANCH_NOT_ASSIGNED";
        public const string BloodRequestBranchMismatch = "BLOOD_REQUEST_BRANCH_MISMATCH";
        public const string EmployeeCannotReviewRequest = "EMPLOYEE_CANNOT_REVIEW_REQUEST";

        public const string ClinicalNotesRequired = "CLINICAL_NOTES_REQUIRED";
        public const string RejectionReasonRequired = "REJECTION_REASON_REQUIRED";
        public const string CancellationReasonRequired = "CANCELLATION_REASON_REQUIRED";

        public const string UnitsNeededInvalid = "UNITS_NEEDED_INVALID";
        public const string NoAvailableBloodUnits = "NO_AVAILABLE_BLOOD_UNITS";
        public const string BeneficiaryNotFoundInNationalRegistry =
            "BENEFICIARY_NOT_FOUND_IN_NATIONAL_REGISTRY";

        public const string BloodTypeRequired = "BLOOD_TYPE_REQUIRED";
        public const string UrgencyLevelRequired = "URGENCY_LEVEL_REQUIRED";

        public const string NoReservedUnitsFound = "NO_RESERVED_UNITS_FOUND";

        // Donation Intent
        public const string ActiveDonationIntentAlreadyExists =
            "ACTIVE_DONATION_INTENT_ALREADY_EXISTS";
        public const string DonorAgeNotAllowed = "DONOR_AGE_NOT_ALLOWED";
        public const string DonorTemporarilyDeferred = "DONOR_TEMPORARILY_DEFERRED";
        public const string DonorPermanentlyDeferred = "DONOR_PERMANENTLY_DEFERRED";
        public const string DonationIntervalNotPassed = "DONATION_INTERVAL_NOT_PASSED";

        public const string BloodRequestNotPublished = "BLOOD_REQUEST_NOT_PUBLISHED";
        public const string BloodRequestNotAvailableForDonation =
            "BLOOD_REQUEST_NOT_AVAILABLE_FOR_DONATION";
        public const string BloodRequestBloodTypeMissing = "BLOOD_REQUEST_BLOOD_TYPE_MISSING";
        public const string BloodRequestUnitsMissing = "BLOOD_REQUEST_UNITS_MISSING";
        public const string BloodRequestDoesNotNeedMoreUnits =
            "BLOOD_REQUEST_DOES_NOT_NEED_MORE_UNITS";

        public const string BloodTypeNotCompatible = "BLOOD_TYPE_NOT_COMPATIBLE";

        public const string DonationIntentNotActive = "DONATION_INTENT_NOT_ACTIVE";

        public const string ScreeningSessionNotFound = "SCREENING_SESSION_NOT_FOUND";
        public const string ScreeningSessionNotEligible = "SCREENING_SESSION_NOT_ELIGIBLE";
        public const string ScreeningSessionNotCompleted = "SCREENING_SESSION_NOT_COMPLETED";
        public const string ScreeningSessionAlreadyUsed = "SCREENING_SESSION_ALREADY_USED";

        public const string EmployeeBranchRequired = "EMPLOYEE_BRANCH_REQUIRED";
        public const string DonationIntentExpired = "DONATION_INTENT_EXPIRED";
        public const string ConfirmedBloodTypeRequired = "CONFIRMED_BLOOD_TYPE_REQUIRED";
        public const string TemporaryDeferralEndDateRequired =
            "TEMPORARY_DEFERRAL_END_DATE_REQUIRED";
        public const string TemporaryDeferralEndDateInvalid = "TEMPORARY_DEFERRAL_END_DATE_INVALID";
        public const string FinalDecisionReasonRequired = "FINAL_DECISION_REASON_REQUIRED";
        public const string UnsupportedFinalEligibilityStatus =
            "UNSUPPORTED_FINAL_ELIGIBILITY_STATUS";

        public const string DonorBloodTypeMustBeConfirmed = "DONOR_BLOOD_TYPE_MUST_BE_CONFIRMED";
        public const string ScreeningReviewAnswersRequired = "SCREENING_REVIEW_ANSWERS_REQUIRED";

        public const string DuplicateScreeningAnswerReview = "DUPLICATE_SCREENING_ANSWER_REVIEW";

        public const string ScreeningAnswerDoesNotBelongToIntent =
            "SCREENING_ANSWER_DOES_NOT_BELONG_TO_INTENT";

        public const string ScreeningReviewRequiredBeforeFinalAssessment =
            "SCREENING_REVIEW_REQUIRED_BEFORE_FINAL_ASSESSMENT";

        // Inventory
        public const string BloodUnitNotFound = "BLOOD_UNIT_NOT_FOUND";
        public const string InvalidBloodUnitStatus = "INVALID_BLOOD_UNIT_STATUS";
        public const string BloodUnitBranchMismatch = "BLOOD_UNIT_BRANCH_MISMATCH";
        public const string InventoryActionNotAllowed = "INVENTORY_ACTION_NOT_ALLOWED";
        public const string BloodUnitDisposalReasonRequired = "BLOOD_UNIT_DISPOSAL_REASON_REQUIRED";
        public const string BloodUnitDeallocationNoteRequired =
            "BLOOD_UNIT_DEALLOCATION_NOTE_REQUIRED";
        public const string BloodUnitAllocationMustBeReleasedFirst =
            "BLOOD_UNIT_ALLOCATION_MUST_BE_RELEASED_FIRST";
        public const string BloodUnitExpired = "BLOOD_UNIT_EXPIRED";
    }
}
