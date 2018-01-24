using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models
{
    public class UserBlockedException : ApplicationException
    {
        public UserBlockedException()
            : base("user blocked by admin")
        {
        }
    }

    public class userIsNotUpdateVersionException : ApplicationException
    {
        public userIsNotUpdateVersionException():base("the user is not update the version from applications store")
        {

        }
    }

    public class UserPermissionException : ApplicationException
    {
        public UserPermissionException()
            : base("no appropriate permission to perform the action")
        {
        }
    }

    public class UserUnauthorizedException : ApplicationException
    {
        public UserUnauthorizedException()
            : base("user unauthorized")
        {
        }
    }

    public class AuthenticationTokenIncorrectException : ApplicationException
    {
        public AuthenticationTokenIncorrectException()
            : base("authentication token is incorrect")
        {
        }
    }

    public class DeviceNotExistException : ApplicationException
    {
        public DeviceNotExistException()
            : base("this device does not exist")
        {
        }
    }

    public class UserNotExistException : ApplicationException
    {
        public UserNotExistException()
            : base("this user does not exist")
        {
        }
    }

    public class UserNotExistInMonitexTableException : ApplicationException
    {
        public UserNotExistInMonitexTableException()
            : base("this user does not exist in monitex table")
        {
        }
    }

    public class couponNotRelevantException : ApplicationException
    {
        public couponNotRelevantException()
            : base("this coupon does not exist")
        {
        }
    }
    public class CantSentMultipleCoupon : ApplicationException
    {
        public CantSentMultipleCoupon()
            : base("cant send multiple coupons to this user")
        {
        }
    }

    public class PhoneNotExistInDataBase : ApplicationException
    {
        public PhoneNotExistInDataBase()
            : base("this user does not exist in database")
        {
        }
    }

    public class PhoneNotExistException : ApplicationException
    {
        public PhoneNotExistException()
            : base("this phone does not exist")
        {
        }
    }

    public class GoogleAPIException : ApplicationException
    {
        public GoogleAPIException()
            : base("google api error")
        {
        }
    }

    public class OrderNotFoundException : ApplicationException
    {
        public OrderNotFoundException()
            : base("this order is not found")
        {
        }
    }


    public class OrderNotRelevantException : ApplicationException
    {
        public OrderNotRelevantException()
            : base("this order is not relevant any more")
        {
        }
    }

    public class orderCannotCanceledException : ApplicationException
    {
        public orderCannotCanceledException()
            : base("this order cannot canceled")
        {
        }
    }

    public class CannotCancelFutureRideException : ApplicationException
    {
        public CannotCancelFutureRideException()
            : base("this future ride cannot canceled")
        {
        }
    }

    

    public class orderCannotReCreatedException : ApplicationException
    {
        public orderCannotReCreatedException()
            : base("this order cannot ReCreated")
        {
        }
    }


    public class OrderHasNoLocation : ApplicationException
    {
        public OrderHasNoLocation()
            : base("somehow, this order has no location")
        {

        }
    }

    public class PaymentWithPayPalCannotCreated : ApplicationException
    {
        public PaymentWithPayPalCannotCreated()
            : base("somehow, this payment can not be made")
        {

        }
    }

    public class UserHasNoLocation : ApplicationException
    {
        public UserHasNoLocation()
            : base("user doesnt have location")
        {

        }
    }
    public class FavoriteNotExistException : ApplicationException
    {
        public FavoriteNotExistException()
            : base("favorite not exist")
        {
        }
    }


    public class PhoneNumberNotValidException : ApplicationException
    {
        public PhoneNumberNotValidException()
            : base("phone number is not valid")
        {
        }
    }



    public class SMSProcessException : ApplicationException
    {
        public SMSProcessException()
            : base("error in send SMS process")
        {
        }
    }

    public class ExpirationException : ApplicationException
    {
        public ExpirationException()
            : base("expiration error")
        {
        }
    }


    public class NoRelevantDataException : ApplicationException
    {
        public NoRelevantDataException()
            : base("There is no relevant information")
        {
        }
    }


    public class ValidateSMSFaildException : ApplicationException
    {
        public ValidateSMSFaildException()
            : base("the code failed in validation test")
        {
        }
    }

    public class UserForbiddenException : ApplicationException
    {
        public UserForbiddenException()
            : base("User is forbidden")
        {
        }
    }

    public class CompanyExistsException : ApplicationException
    {
        public CompanyExistsException() :
            base("Company exists. Can not add new one")
        {
        }
    }

    public class CompanyNotExistsException : ApplicationException
    {
        public CompanyNotExistsException() :
            base("Company not exists")
        {
        }
    }

    public class PhoneAlreadyRegisteredToCompany : ApplicationException
    {
        public PhoneAlreadyRegisteredToCompany() :
            base("This phone already registered to this company")
        { }
    }

    public class PhoneAlreadyExistException : ApplicationException
    {
        public PhoneAlreadyExistException() :
            base("This phone already Exist")
        { }
    }


    public class PhoneNotRegistetedToCompany : ApplicationException
    {
        public PhoneNotRegistetedToCompany() :
            base("This phone does not registered to this company")
        { }
    }
}