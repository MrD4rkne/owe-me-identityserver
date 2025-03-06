﻿using Microsoft.AspNetCore.Identity;
using Moq;

namespace OweMe.Identity.UnitTests;

internal static class UserManagerMockHelper
{
    public static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        return mgr;
    }
}