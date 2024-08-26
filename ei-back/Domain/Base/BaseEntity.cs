﻿namespace ei_back.Domain.Base
{
    public class BaseEntity
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public void SetCreatedDate(DateTime createdDate)
        {
            CreatedAt = createdDate;
            UpdatedAt = createdDate;
        }

        public void SetUpdatedDate(DateTime updatedDate)
        {
            UpdatedAt= updatedDate;
        }

    }
}
