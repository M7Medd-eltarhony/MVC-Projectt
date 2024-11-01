﻿using AutoMapper;
using Demo.BLL.Interfaces;
using Demo.BLL.Repositories;
using Demo.DAL.Models;
using Demo.PL.Helpers;
using Demo.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.PL.Controllers
{
	[Authorize]
	public class EmployeeController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public EmployeeController(IUnitOfWork unitOfWork,
			IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}
		public async Task<IActionResult> Index(string SearchValue)
		{
			IEnumerable<Employee> employees;
			if (string.IsNullOrEmpty(SearchValue))
				employees = await _unitOfWork.EmployeeRepository.GetAllAsync();
			else
				employees = _unitOfWork.EmployeeRepository.GetEmployeeByName(SearchValue);

			var MappedEmployees = _mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeViewModel>>(employees);
			return View(MappedEmployees);
		}
		public IActionResult Create()
		{
			//ViewBag.Departments = _departmentRepository.GetAll();
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(EmployeeViewModel employeeVM)
		{
			if (ModelState.IsValid)
			{
				employeeVM.ImageName = DocumentSettings.UploadFile(employeeVM.Image, "Images");

				var MappedEmployee = _mapper.Map<EmployeeViewModel, Employee>(employeeVM);
				await _unitOfWork.EmployeeRepository.AddAsync(MappedEmployee);
				var Result = await _unitOfWork.CompleteAsync();
				if (Result > 0)
				{
					TempData["Message"] = "Employee Is Created";
				}
				return RedirectToAction(nameof(Index));
			}
			return View(employeeVM);
		}
		public async Task<IActionResult> Details(int? id, string ViewName = "Details")
		{
			if (id is null)
				return BadRequest();
			var employee = await _unitOfWork.EmployeeRepository.GetByIdAsync(id.Value);
			if (employee is null)
				return NotFound();
			var MappedEmployee = _mapper.Map<Employee, EmployeeViewModel>(employee);
			return View(ViewName, MappedEmployee);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int? id)
		{
			return await Details(id, "Edit");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(EmployeeViewModel employeeVM, [FromRoute] int id)
		{
			if (id != employeeVM.Id)
				return BadRequest();
			if (ModelState.IsValid)
			{
				try
				{
					if (employeeVM.Image is not null)
					{
						employeeVM.ImageName = DocumentSettings.UploadFile(employeeVM.Image, "Images");
					}
					var MappedEmployee = _mapper.Map<Employee>(employeeVM);
					_unitOfWork.EmployeeRepository.Update(MappedEmployee);
					await _unitOfWork.CompleteAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (System.Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}
			return View(employeeVM);
		}
		public async Task<IActionResult> Delete(int? id)
		{
			return await Details(id, "Delete");
		}
		[HttpPost]
		public async Task<IActionResult> Delete(EmployeeViewModel employeeVM, [FromRoute] int id)
		{
			if (id != employeeVM.Id)
				return BadRequest();
			try
			{
				var MappedEmployee = _mapper.Map<EmployeeViewModel, Employee>(employeeVM);
				_unitOfWork.EmployeeRepository.Delete(MappedEmployee);
				var Result = await _unitOfWork.CompleteAsync();
				if (Result > 0 && employeeVM.ImageName is not null)
				{
					DocumentSettings.DeleteFile(employeeVM.ImageName, "Images");
				}
				return RedirectToAction(nameof(Index));
			}
			catch (System.Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return View(employeeVM);
			}
		}

	}
}