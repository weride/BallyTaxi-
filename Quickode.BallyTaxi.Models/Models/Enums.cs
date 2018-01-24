using System.ComponentModel.DataAnnotations;

namespace Quickode.BallyTaxi.Models
{
    public enum UserType : int
    {
        Driver = 0,
        Passenger = 1
    }

    public enum RoleAccountForAdmin : int
    {
        admin = 1,
        shekem = 2,
        kastle = 3
    }

    public enum OrderStatus : int
    {
        [Display(Name = "Pending")]
        Pending = 0,
        [Display(Name = "Confirmed")]
        Confirmed = 1,
        [Display(Name = "Canceled")]
        Canceled = 2,
        [Display(Name = "Completed")]
        Completed = 3,
        [Display(Name = "Dissatisfied")]
        Dissatisfied = 4,
        [Display(Name = "Payment")]
        Payment = 5,
        [Display(Name = "DisputeAmount")]
        DisputeAmount = 6,
        [Display(Name = "PaymentError")]
        PaymentError = 7,
        [Display(Name = "DriverCanceled")]
        DriverDeclined = 8
    }

    public enum SMSType : int
    {
        DriverFoundSMS = 0,
        DriverArrived = 1,
        PayWithBuissness = 2,
        DriverNotFound = 3,
        riderCancelled = 4,
        ReminderForFutureRide = 5,
        CouponTextForSMS = 6
    }

    public enum ErrorId : int
    {
        driverBecomeAvailableInTheMiddleOfTheTravel = 1
    }
    public enum Order_DriverStatus : int
    {
        SentPush = 1,
        Accepted = 2,
        Declined = 3,
        Skipped = 4,
        Cancelled = 5,
        Standby = 6
    }

    public enum OrderType : int
    {
        Search = 0,
        Current = 1,
        Past = 2,
        Future = 3
    }

    public enum FlowSteps : int
    {
        Step1 = 1,
        Step2 = 2,
        Step3 = 3,
        Step4 = 4,
        Step5 = 5
    }

    public enum DriverPaymentStatus : int
    {
        Free = 1,
        HasNoPaymentDetails = 2,
        HasPaymentDetails = 3
    }
    public enum DriverStatus : int
    {
        Available = 0,
        HasRequestAsFirst = 1,
        HasRequest = 2,
        NotAvailable = 3,
        OnTheWayToPickupLocation = 4,
        InPickupLocation = 5,
        OnTheWayToDestination = 6,
        PendingAcceptRequest = 7
    }

    public enum PlatformTypes : int
    {
        IOS = 0,
        Android = 1,
        WP = 2
    }

    public enum updateLocationStatus : int
    {
        sent = 1,
        updateLocation = 2,
        notUpdateAfter3Times = 3
    }

    public enum DriverEmail : int
    {
        RegisterTitle = 0,
        Register1 = 1,
        Register2 = 2,
        Register3 = 3,
        endMail = 4,
        CompleteTripTitle = 5,
        CompleteTrip1 = 6,
        CompleteTrip2 = 7,
        CanceledTripTitle = 8,
        CanceledTrip1 = 9,
        CanceledTrip2 = 10
    }

    public enum PassengerEmail : int
    {
        CompleteTripTitle = 1,
        CompletedTripPass1 = 2,
        CompletedTripPass2 = 3,
        CanceledTripTitle = 4,
        CanceledTripForPass1 = 5,
        CanceledTripForPass2 = 6
    }

    public enum PassengerNotificationTypes : int
    {
        DriverFound = 1,
        DriverArrived = 2,
        DriverNotFound = 3,
        PaymentSuccessful = 4,
        PaymentError = 5,
        NoCreditCard = 6,
        ChooseCreditCard = 7,
        PayInTaxi = 8,
        DriverCancelledRide = 9,
        PayWithPayPal = 10,
        paypalPassengerError = 11,
        PayWithBuissness = 12,
        ReminderForFutureRide = 13,
        DriverArriveInFutureRide = 14,
        CreditCardPassengerError = 15,
        notFoundTaxiWithDiscount = 16,
        notFoundTaxiWithHandicapped = 17,
        notFoundTaxiWithCourier = 18,
        DriverCancelledFutureRide = 19,
        massageForPassenger=20,
        driverNotFoundFirst=21,
        FutureRideCannotCancelled=22,
        openAdvertising = 23,
        RidePrice = 24,
        PassengerRefuseRidePrice = 25
    }

    public enum DriverNotificationTypes : int
    {
        NewRideRequest = 10,
        UserCancelRideRequest = 11,
        RideNotRelevantAnymore = 12,
        ReminderForFutureRide = 13,
        RideAllocated = 14,
        AmountDispute = 15,
        PaymentSuccessful = 16,
        PaymentError = 17,
        paypalDriverError = 18,
        creaditCardDriverError = 19,
        PaymentSuccessfulAndCoupon = 20,
        sendToGetLocation = 21,
        UserCancelFutureRideRequest = 22,
        massageForDriver=23,
        FutureRideCannotCancelled = 24,//only for server
        recivedFutureRide=25,
        openAdvertising=26,
        PassengerRefuseRidePrice = 27,
        RideEndedSuccessfull = 28,
        RideEndedSuccessfullAndTip = 29
        //creaditCardDriverError=20
    }

    public enum SMSTypeToSend : int
    {
        AllDrivers = 1,
        AllPassengers = 2,
        DriversNotEndRegistration = 3,
        ToAllDriverToUpdateTheApp = 4
    }

    public enum UserLanguages : int
    {
        he = 1,
        en = 2,
        es = 3,//spanish
        fr = 4,//France
        pt = 5,//portugies
        it = 6,//italiano
        ru = 7//russian
    }

    public enum DriverQueue : int
    {
        InQueue = 0,
        GotRide = 1,
        NotInQueue = 2 //currently not in use
    }

    /*public enum PaymentMethod : int
    {
        InTaxi = 0,
        InApp = 1
    }*/
    public enum DriverPayment : int
    {
        paypal = 1,
        creditCard = 2,
        bankAccount = 3
    }

    public enum DriverPaymentMethod : int
    {
        perMonth = 1,
        perTravel = 2,
        newMonthPayment=3

    }

    public enum CustomerPaymentMethod : int
    {
        //Cash = 0,
        //CreditCard = 1,
        //InApp = 2

        [Display(Name = "Paypal")]
        Paypal = 1,
        [Display(Name = "Cash")]
        Cash = 2,
        [Display(Name = "CreditCard")]
        CreditCard = 3,
        [Display(Name = "Business")]
        Business = 4,
        [Display(Name = "InApp")]
        InApp = 5
    }

    public enum Currency : int
    {
        NIS = 1,
        USD = 2,
        EUR = 3
    }

    public enum updateCreditCardType : int
    {
        delete = 1,
        setDefault = 2
    }


}