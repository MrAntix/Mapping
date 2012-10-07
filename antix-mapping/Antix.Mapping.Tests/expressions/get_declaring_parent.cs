using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antix.Mapping.Expressions;
using Antix.Mapping.Tests.Models;
using Xunit;

namespace Antix.Mapping.Tests.expressions
{
    public class get_declaring_parent
    {
        [Fact]
        public void gets_simple_expression()
        {
            var obj = new Person
                          {
                              Addresses = new[]
                                              {
                                                  new Address()
                                              }
                          };

            var exp = (Expression<Func<IEnumerable<Address>>>)
                      (() => obj.Addresses);

            var sut = new CollapseMembersExpressionVisitor();

            var result = sut.Modify(exp);

            Assert.Equal("Addresses", result.Member.Name);
            Assert.Equal(obj, result.Subject);
        }

        [Fact]
        public void gets_anonmous_wrapped_expression()
        {
            var obj = new
                          {
                              dooda = new
                                          {
                                              it = new Person
                                                       {
                                                           Addresses = new[]
                                                                           {
                                                                               new Address()
                                                                           }
                                                       }
                                          }
                          };

            var exp = (Expression<Func<IEnumerable<Address>>>)
                      (() => obj.dooda.it.Addresses);

            var sut = new CollapseMembersExpressionVisitor();

            var result = sut.Modify(exp);

            Assert.Equal("Addresses", result.Member.Name);
            Assert.Equal(obj.dooda.it, result.Subject);
        }

        [Fact]
        public void gets_simple_expression_with_interfaces()
        {
            var obj = (IPerson) new Person
                                    {
                                        Addresses = new[]
                                                        {
                                                            new Address()
                                                        }
                                    };

            var exp = (Expression<Func<IEnumerable<IAddress>>>)
                      (() => obj.Addresses);

            var sut = new CollapseMembersExpressionVisitor();

            var result = sut.Modify(exp);

            Assert.Equal("Addresses", result.Member.Name);
            Assert.Equal(obj, result.Subject);
        }


        [Fact]
        public void fails_with_lambda_invoke()
        {
            var obj = new Person
                          {
                              Addresses = new[]
                                              {
                                                  new Address()
                                              }
                          };
            var lambda = (Func<IEnumerable<Address>>)
                         (() => obj.Addresses);

            var exp = (Expression<Func<IEnumerable<Address>>>)
                      (() => lambda());

            var sut = new CollapseMembersExpressionVisitor();

            var ex = Assert.Throws<NotSupportedException>(() => sut.Modify(exp));
        }
    }
}