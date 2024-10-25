using System.Collections.Generic;
using System;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{

    partial class StreamUpLib
    {

        public List<GroupUser> GetRandomUsersFromGroup(string groupName, int count = 1)
        {
            if (!_CPH.GroupExists(groupName))
            {
                _CPH.AddGroup(groupName);
            }
            List<GroupUser> usersInGroup = _CPH.UsersInGroup(groupName);
            List<GroupUser> users = [];
            if (usersInGroup.Count == 0)
            {
                
                LogError("No Users In Said Group");
                return users;
            }
            else if (usersInGroup.Count == 1)
            {
                users.Add(usersInGroup[0]);
                return users;
            }
            else
            {
                List<int> randomIndex = GetRandomNumbers(0,usersInGroup.Count,count);
             
                foreach(int i in randomIndex)
                {
                    users.Add(usersInGroup[i]);
                }

                
                return users;
            }

        }

    }

}