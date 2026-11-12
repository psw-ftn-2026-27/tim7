using AutoMapper;
using Payment.Application.WalletAdministration;
using Payment.Application.WalletBrowsing;
using Payment.Domain.Wallets;

namespace Payment.Application;

public sealed class PaymentMapperProfile : Profile
{
    public PaymentMapperProfile()
    {
        CreateMap<Wallet, WalletDto>()
            .ForMember(dto => dto.Balance, options => options.MapFrom(wallet => wallet.Balance()));
        CreateMap<Transaction, TransactionDto>();
        CreateMap<Wallet, UserBalanceDto>()
            .ForMember(dto => dto.Balance, options => options.MapFrom(wallet => wallet.Balance()));
    }
}
