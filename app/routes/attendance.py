from flask import Blueprint, render_template, request, redirect, url_for, flash
from app import db
from app.models import AttendanceRecord, Employee
from datetime import date

attendance_bp = Blueprint('attendance', __name__)


@attendance_bp.route('/')
def index():
    selected_date_str = request.args.get('date', date.today().isoformat())
    dept_id = request.args.get('dept_id', '')

    try:
        selected_date = date.fromisoformat(selected_date_str)
    except ValueError:
        selected_date = date.today()

    from app.models import Department
    departments = Department.query.order_by(Department.name).all()

    emp_query = Employee.query.filter_by(status='active')
    if dept_id:
        emp_query = emp_query.filter_by(department_id=int(dept_id))
    employees = emp_query.order_by(Employee.employee_code).all()

    attendance_map = {}
    for rec in AttendanceRecord.query.filter_by(date=selected_date).all():
        attendance_map[rec.employee_id] = rec

    return render_template(
        'attendance/index.html',
        employees=employees,
        attendance_map=attendance_map,
        departments=departments,
        selected_date=selected_date,
        dept_id=dept_id,
    )


@attendance_bp.route('/them', methods=['GET', 'POST'])
def create():
    employees = Employee.query.filter_by(status='active').order_by(
        Employee.employee_code).all()

    if request.method == 'POST':
        employee_id = request.form.get('employee_id')
        date_str = request.form.get('date', '').strip()
        check_in = request.form.get('check_in', '').strip()
        check_out = request.form.get('check_out', '').strip()
        status = request.form.get('status', 'present')
        note = request.form.get('note', '').strip()

        if not employee_id or not date_str:
            flash('Vui lòng chọn nhân viên và ngày.', 'danger')
            return render_template('attendance/form.html', employees=employees,
                                   form_data=request.form)

        try:
            rec_date = date.fromisoformat(date_str)
        except ValueError:
            flash('Ngày không hợp lệ.', 'danger')
            return render_template('attendance/form.html', employees=employees,
                                   form_data=request.form)

        existing = AttendanceRecord.query.filter_by(
            employee_id=int(employee_id), date=rec_date
        ).first()
        if existing:
            flash('Đã có bản ghi chấm công cho nhân viên này vào ngày đã chọn.', 'warning')
            return render_template('attendance/form.html', employees=employees,
                                   form_data=request.form)

        rec = AttendanceRecord(
            employee_id=int(employee_id),
            date=rec_date,
            check_in=check_in or None,
            check_out=check_out or None,
            status=status,
            note=note or None,
        )
        db.session.add(rec)
        db.session.commit()
        flash('Thêm chấm công thành công!', 'success')
        return redirect(url_for('attendance.index', date=rec_date.isoformat()))

    return render_template('attendance/form.html', employees=employees, form_data=None)


@attendance_bp.route('/sua/<int:rec_id>', methods=['GET', 'POST'])
def update(rec_id):
    rec = AttendanceRecord.query.get_or_404(rec_id)
    employees = Employee.query.filter_by(status='active').order_by(
        Employee.employee_code).all()

    if request.method == 'POST':
        rec.check_in = request.form.get('check_in', '').strip() or None
        rec.check_out = request.form.get('check_out', '').strip() or None
        rec.status = request.form.get('status', 'present')
        rec.note = request.form.get('note', '').strip() or None
        db.session.commit()
        flash('Cập nhật chấm công thành công!', 'success')
        return redirect(url_for('attendance.index', date=rec.date.isoformat()))

    return render_template('attendance/form.html', rec=rec, employees=employees,
                           form_data=None)


@attendance_bp.route('/xoa/<int:rec_id>', methods=['POST'])
def delete(rec_id):
    rec = AttendanceRecord.query.get_or_404(rec_id)
    rec_date = rec.date
    db.session.delete(rec)
    db.session.commit()
    flash('Đã xóa bản ghi chấm công.', 'success')
    return redirect(url_for('attendance.index', date=rec_date.isoformat()))


@attendance_bp.route('/hang-loat', methods=['POST'])
def bulk_save():
    """Save attendance for all employees on a date."""
    selected_date_str = request.form.get('date', date.today().isoformat())
    try:
        selected_date = date.fromisoformat(selected_date_str)
    except ValueError:
        flash('Ngày không hợp lệ.', 'danger')
        return redirect(url_for('attendance.index'))

    employee_ids = request.form.getlist('employee_ids')
    for eid in employee_ids:
        eid = int(eid)
        status = request.form.get(f'status_{eid}', 'absent')
        check_in = request.form.get(f'check_in_{eid}', '').strip() or None
        check_out = request.form.get(f'check_out_{eid}', '').strip() or None
        note = request.form.get(f'note_{eid}', '').strip() or None

        rec = AttendanceRecord.query.filter_by(
            employee_id=eid, date=selected_date
        ).first()
        if rec:
            rec.status = status
            rec.check_in = check_in
            rec.check_out = check_out
            rec.note = note
        else:
            rec = AttendanceRecord(
                employee_id=eid,
                date=selected_date,
                check_in=check_in,
                check_out=check_out,
                status=status,
                note=note,
            )
            db.session.add(rec)

    db.session.commit()
    flash(f'Đã lưu chấm công ngày {selected_date.strftime("%d/%m/%Y")}.', 'success')
    return redirect(url_for('attendance.index', date=selected_date.isoformat()))


@attendance_bp.route('/bao-cao')
def report():
    month = int(request.args.get('month', date.today().month))
    year = int(request.args.get('year', date.today().year))

    from app.models import Department
    departments = Department.query.order_by(Department.name).all()
    dept_id = request.args.get('dept_id', '')

    emp_query = Employee.query.filter_by(status='active')
    if dept_id:
        emp_query = emp_query.filter_by(department_id=int(dept_id))
    employees = emp_query.order_by(Employee.employee_code).all()

    import calendar as _cal
    first_day = date(year, month, 1)
    _, last_day_num = _cal.monthrange(year, month)
    last_day = date(year, month, last_day_num)

    report_data = []
    for emp in employees:
        records = {
            r.date.day: r
            for r in AttendanceRecord.query.filter(
                AttendanceRecord.employee_id == emp.id,
                AttendanceRecord.date >= first_day,
                AttendanceRecord.date <= last_day,
            ).all()
        }
        present = sum(1 for r in records.values() if r.status in ('present', 'late'))
        absent = sum(1 for r in records.values() if r.status == 'absent')
        leave = sum(1 for r in records.values() if r.status == 'leave')
        late = sum(1 for r in records.values() if r.status == 'late')
        report_data.append({
            'employee': emp,
            'records': records,
            'present': present,
            'absent': absent,
            'leave': leave,
            'late': late,
        })

    return render_template(
        'attendance/report.html',
        report_data=report_data,
        month=month,
        year=year,
        days_in_month=last_day_num,
        departments=departments,
        dept_id=dept_id,
    )
