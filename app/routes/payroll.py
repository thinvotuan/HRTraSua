from flask import Blueprint, render_template, request, redirect, url_for, flash
from app import db
from app.models import PayrollRecord, Employee, AttendanceRecord
from datetime import date
import calendar

payroll_bp = Blueprint('payroll', __name__)


@payroll_bp.route('/')
def index():
    month = int(request.args.get('month', date.today().month))
    year = int(request.args.get('year', date.today().year))
    dept_id = request.args.get('dept_id', '')

    from app.models import Department
    departments = Department.query.order_by(Department.name).all()

    emp_query = Employee.query.filter_by(status='active')
    if dept_id:
        emp_query = emp_query.filter_by(department_id=int(dept_id))
    employees = emp_query.order_by(Employee.employee_code).all()

    payroll_map = {
        r.employee_id: r
        for r in PayrollRecord.query.filter_by(month=month, year=year).all()
    }

    total_net = sum(
        r.net_salary for r in payroll_map.values()
    )

    return render_template(
        'payroll/index.html',
        employees=employees,
        payroll_map=payroll_map,
        departments=departments,
        month=month,
        year=year,
        dept_id=dept_id,
        total_net=total_net,
    )


@payroll_bp.route('/tao-bang-luong', methods=['POST'])
def generate():
    """Auto-generate payroll records for a month."""
    month = int(request.form.get('month', date.today().month))
    year = int(request.form.get('year', date.today().year))

    _, days_in_month = calendar.monthrange(year, month)
    # Working days (Mon-Fri)
    standard_days = sum(
        1 for d in range(1, days_in_month + 1)
        if date(year, month, d).weekday() < 5
    )

    employees = Employee.query.filter_by(status='active').all()
    created = 0

    import calendar as _cal
    first_day = date(year, month, 1)
    _, last_day_num = _cal.monthrange(year, month)
    last_day = date(year, month, last_day_num)

    for emp in employees:
        existing = PayrollRecord.query.filter_by(
            employee_id=emp.id, month=month, year=year
        ).first()
        if existing:
            continue

        # Count work days from attendance
        work_days = AttendanceRecord.query.filter(
            AttendanceRecord.employee_id == emp.id,
            AttendanceRecord.status.in_(['present', 'late', 'half_day', 'leave']),
            AttendanceRecord.date >= first_day,
            AttendanceRecord.date <= last_day,
        ).count()

        base = emp.position.base_salary if emp.position else 0

        rec = PayrollRecord(
            employee_id=emp.id,
            month=month,
            year=year,
            base_salary=base,
            allowance=0,
            bonus=0,
            deduction=0,
            work_days=work_days,
            standard_days=standard_days,
            status='draft',
        )
        db.session.add(rec)
        created += 1

    db.session.commit()
    flash(f'Đã tạo bảng lương tháng {month}/{year} cho {created} nhân viên.', 'success')
    return redirect(url_for('payroll.index', month=month, year=year))


@payroll_bp.route('/<int:rec_id>/sua', methods=['GET', 'POST'])
def update(rec_id):
    rec = PayrollRecord.query.get_or_404(rec_id)

    if request.method == 'POST':
        try:
            rec.allowance = float(request.form.get('allowance', 0) or 0)
            rec.bonus = float(request.form.get('bonus', 0) or 0)
            rec.deduction = float(request.form.get('deduction', 0) or 0)
            rec.work_days = int(request.form.get('work_days', 0) or 0)
            rec.standard_days = int(request.form.get('standard_days', 26) or 26)
            rec.note = request.form.get('note', '').strip() or None
            rec.status = request.form.get('status', 'draft')
        except (ValueError, TypeError):
            flash('Dữ liệu không hợp lệ.', 'danger')
            return render_template('payroll/form.html', rec=rec)

        db.session.commit()
        flash('Cập nhật lương thành công!', 'success')
        return redirect(url_for('payroll.index', month=rec.month, year=rec.year))

    return render_template('payroll/form.html', rec=rec)


@payroll_bp.route('/xac-nhan-thanh-toan', methods=['POST'])
def mark_paid():
    month = int(request.form.get('month', date.today().month))
    year = int(request.form.get('year', date.today().year))

    PayrollRecord.query.filter_by(month=month, year=year, status='draft').update(
        {'status': 'paid'}
    )
    db.session.commit()
    flash(f'Đã xác nhận thanh toán lương tháng {month}/{year}.', 'success')
    return redirect(url_for('payroll.index', month=month, year=year))
