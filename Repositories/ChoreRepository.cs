using Microsoft.Data.SqlClient;
using Roommates.Models;
using System;
using System.Collections.Generic;

namespace Roommates.Repositories
{
    public class ChoreRepository : BaseRepository
    {
        public ChoreRepository(string connectionString) : base(connectionString) { }
        public List<Chore> GetAll()
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM Chore";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Chore> chores = new List<Chore>();
                        while (reader.Read())
                        {
                            int idColumnPosition = reader.GetOrdinal("Id");
                            int idValue = reader.GetInt32(idColumnPosition);

                            int nameColumnPosition = reader.GetOrdinal("Name");
                            string nameValue = reader.GetString(nameColumnPosition);

                            Chore chore = new Chore()
                            {
                                Id = idValue,
                                Name = nameValue,
                            };
                            chores.Add(chore);
                        }
                        return chores;
                    }
                }
            }
        }
        public Chore GetById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Name FROM Chore WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Chore chore = null;

                        if (reader.Read())
                        {
                            chore = new Chore()
                            {
                                Id = id,
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                            };
                        }
                        return chore;
                    }
                }
            }
        }
        public void Insert(Chore chore)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Chore (Name)
                                                OUTPUT INSERTED.Id
                                                VALUES (@name)";
                    cmd.Parameters.AddWithValue("@name", chore.Name);
                    int id = (int)cmd.ExecuteScalar();

                    chore.Id = id;
                }
            }
        }
        public List<Chore> GetUnassignedChores()
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Chore.Id, Chore.Name FROM Chore
                                        LEFT JOIN RoommateChore ON Chore.Id = RoommateChore.ChoreId
                                        WHERE RoommateChore.ChoreId IS NULL";
                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Chore> chores = new List<Chore>();
                        while (reader.Read())
                        {
                            int idColumnPosition = reader.GetOrdinal("Id");
                            int idValue = reader.GetInt32(idColumnPosition);

                            int nameColumnPosition = reader.GetOrdinal("Name");
                            string nameValue = reader.GetString(nameColumnPosition);

                            Chore chore = new Chore()
                            {
                                Id = idValue,
                                Name = nameValue,
                            };
                            chores.Add(chore);
                        }
                        return chores;
                    }
                }
            }
        }
        public void AssignChore(int roommateId, int choreId)
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO RoommateChore (RoommateId, ChoreId)
                                        OUTPUT INSERTED.ID
                                        VALUES (@roommateId, @choreId)";
                    cmd.Parameters.AddWithValue("@roommateId", roommateId);
                    cmd.Parameters.AddWithValue("@choreId", choreId);
                    int id = (int)cmd.ExecuteScalar();
                    if (id != default)
                    {
                        Console.WriteLine("This chore was successfully assigned.");
                    }
                }
            }
        }
        
        public List<RoommateChore> GetChoreCounts()
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT FirstName, COUNT(RoommateChore.RoommateId) as ChoresPerRoommate FROM RoommateChore
                                        LEFT JOIN Roommate ON RoommateChore.RoommateId = Roommate.Id
                                        GROUP BY Roommate.FirstName;";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<RoommateChore> roommates = new List<RoommateChore>();
                        RoommateChore roommate = null;

                        while(reader.Read())
                        {
                            roommate = new RoommateChore
                            {
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),                          
                                ChoresPerRoommate = reader.GetInt32(reader.GetOrdinal("ChoresPerRoommate")),
                            };
                            roommates.Add(roommate);
                        }

                        return roommates;
                    }
                }
            }
        }
    }
}
