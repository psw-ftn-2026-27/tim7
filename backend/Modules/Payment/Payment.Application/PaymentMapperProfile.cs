using AutoMapper;
using Payment.Application.TourPurchasing;
using Payment.Application.WalletAdministration;
using Payment.Application.WalletBrowsing;
using Payment.Domain.Carts;
using Payment.Domain.Wallets;

namespace Payment.Application;

public sealed class PaymentMapperProfile : Profile
{
    public PaymentMapperProfile()
    {
        CreateMap<Wallet, WalletDto>()
            .ForMember(dto => dto.Balance, options => options.MapFrom(wallet => wallet.Balance()));
        CreateMap<Transaction, TransactionDto>();
        CreateMap<OrderItem, CartItemDto>();
    }
}
