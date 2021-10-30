using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    public class InquiryController : Controller
    {
        private readonly IInquiryHeaderRepository _repoInqHeader;
        private readonly IInquiryDetailRepository _repoInqDetail;
        public InquiryController(IInquiryHeaderRepository repoInqHeader,
            IInquiryDetailRepository repoInqDetail)
        {
            _repoInqHeader = repoInqHeader;
            _repoInqDetail = repoInqDetail;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
