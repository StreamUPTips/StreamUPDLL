using System.Collections.Generic;
using System;

namespace StreamUP
{

    partial class StreamUpLib
    {

        public bool GetRandomUsersFromGroup(string groupName, int count = 1, out var users)
        {
            if (!_CPH.GroupExists(groupName))
            {
                _CPH.AddGroup(groupName);
            }
            List<GroupUser> usersInGroup = _CPH.UsersInGroup(groupName);
            if (usersInGroup.Count == 0)
            {
                var users = null;
                LogError("No Users In Said Group");
                return false;
            }
            else if (usersInGroup.Count == 1)
            {
                var users = usersInGroup[0];
            }
            else
            {
                List<int> randomIndex = GetRandomNumbers(0,usersInGroup.Count,count);
                var users;
                foreach(int i in randomIndex)
                {
                    users.Add(usersInGroup[i]);
                }
                return users;
            }

        }

    }

}