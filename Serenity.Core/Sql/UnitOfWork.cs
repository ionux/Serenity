﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Serenity.Data
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private Action _commit;
        private Action _rollback;

        public UnitOfWork(IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            _connection = connection;
            _transaction = SqlTransactions.BeginTransaction(connection);
        }

        public IDbConnection Connection
        {
            get { return _connection; }
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction = null;

                if (_rollback != null)
                {
                    _rollback();
                    _rollback = null;
                }
            }
        }

        public void Commit()
        {
            if (_transaction == null)
                throw new ArgumentNullException("transaction");

            _transaction.Commit();
            _transaction = null;

            if (_commit != null)
            {
                _commit();
                _commit = null;
            }
        }

        public event Action OnCommit
        {
            add { _commit += value; }
            remove { _commit -= value; }
        }

        public event Action OnRollback
        {
            add { _rollback += value; }
            remove { _rollback -= value; }
        }

        public static T Wrap<T>(Func<IUnitOfWork, T> handler)
        {
            using (var connection = SqlConnections.New())
            using (var unitOfWork = new UnitOfWork(connection))
            {
                var response = handler(unitOfWork);
                unitOfWork.Commit();
                return response;
            }
        }
    }
}
