namespace MRK
{
    public class OperationalTransformer
    {
        public Operation Transform(Operation localOp, Operation remoteOp)
        {
            if (localOp is InsertOperation localInsert && remoteOp is InsertOperation remoteInsert)
            {
                return TransformInsertInsert(localInsert, remoteInsert);
            }
            else if (localOp is DeleteOperation localDelete && remoteOp is DeleteOperation remoteDelete)
            {
                return TransformDeleteDelete(localDelete, remoteDelete);
            }
            else if (localOp is InsertOperation insert && remoteOp is DeleteOperation delete)
            {
                return TransformInsertDelete(insert, delete);
            }
            else if (localOp is DeleteOperation del && remoteOp is InsertOperation ins)
            {
                return TransformDeleteInsert(del, ins);
            }

            throw new InvalidOperationException("Unsupported operation types for transformation.");
        }

        private InsertOperation TransformInsertInsert(InsertOperation local, InsertOperation remote)
        {
            if (local.Position < remote.Position || (local.Position == remote.Position && local.Timestamp < remote.Timestamp))
            {
                return local;
            }
            else
            {
                return new InsertOperation(local.Position + remote.Text.Length, local.Text, local.UserId)
                {
                    Timestamp = local.Timestamp
                };
            }
        }

        private DeleteOperation TransformDeleteDelete(DeleteOperation local, DeleteOperation remote)
        {
            if (local.Position + local.Length <= remote.Position)
            {
                return local;
            }
            else if (local.Position >= remote.Position + remote.Length)
            {
                return new DeleteOperation(local.Position - remote.Length, local.Length, local.UserId)
                {
                    Timestamp = local.Timestamp
                };
            }
            else
            {
                int overlapStart = Math.Max(local.Position, remote.Position);
                int overlapEnd = Math.Min(local.Position + local.Length, remote.Position + remote.Length);

                if (local.Position < remote.Position)
                {
                    return new DeleteOperation(local.Position, overlapStart - local.Position, local.UserId)
                    {
                        Timestamp = local.Timestamp
                    };
                }
                else
                {
                    return new DeleteOperation(remote.Position, overlapEnd - remote.Position, local.UserId)
                    {
                        Timestamp = local.Timestamp
                    };
                }
            }
        }

        private Operation TransformInsertDelete(InsertOperation insert, DeleteOperation delete)
        {
            if (insert.Position <= delete.Position)
            {
                return insert;
            }
            else
            {
                return new InsertOperation(insert.Position - delete.Length, insert.Text, insert.UserId)
                {
                    Timestamp = insert.Timestamp
                };
            }
        }

        private Operation TransformDeleteInsert(DeleteOperation delete, InsertOperation insert)
        {
            if (delete.Position >= insert.Position)
            {
                return new DeleteOperation(delete.Position + insert.Text.Length, delete.Length, delete.UserId)
                {
                    Timestamp = delete.Timestamp
                };
            }
            else
            {
                return delete;
            }
        }
    }
}
